using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XmlDocConverter.Fluent
{
	public struct DocumentSourceMemberEntryKey : IEquatable<DocumentSourceMemberEntryKey>
	{
		public DocumentSourceMemberEntryKey(Assembly assembly, string id)
		{
			Contract.Ensures(assembly != null);
			Contract.Requires(id != null);

			Assembly = assembly;
			Id = id;
		}

		public readonly Assembly Assembly;
		public readonly string Id;

		/// <summary>
		/// Override the GetHashCode method.
		/// </summary>
		/// <returns>The hash code for this key.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				hash = hash * 31 + Assembly.GetHashCode();
				hash = hash * 31 + Id.GetHashCode();
				return hash;
			}
		}

		public bool Equals(DocumentSourceMemberEntryKey other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			return obj is DocumentSourceMemberEntryKey && this == (DocumentSourceMemberEntryKey)obj;
		}

		public static bool operator ==(DocumentSourceMemberEntryKey a, DocumentSourceMemberEntryKey b)
		{
			return a.Assembly == b.Assembly
				&& a.Id == b.Id;
		}

		public static bool operator !=(DocumentSourceMemberEntryKey a, DocumentSourceMemberEntryKey b)
		{
			return !(a == b);
		}
	}

	public class DocumentSourceMemberEntry
	{
		public DocumentSourceMemberEntry(Assembly assembly, XElement xElement, XElement rawXElement, string id)
		{
			Contract.Requires(assembly != null);
			Contract.Requires(xElement != null);
			Contract.Requires(rawXElement != null);
			Contract.Requires(!String.IsNullOrWhiteSpace(id));
			Contract.Requires(id.Length > 2);
			Contract.Requires(id[1] == ':');

			Assembly = assembly;
			XElement = xElement;
			RawXElement = rawXElement;
			Id = id;
		}

		/// <summary>
		/// The assembly for this document source entry.
		/// </summary>
		public readonly Assembly Assembly;

		/// <summary>
		/// The XElement containing this member data.
		/// </summary>
		public readonly XElement XElement;

		/// <summary>
		/// The XElement containing this member data exactly as it appears in the XML file.
		/// </summary>
		public readonly XElement RawXElement;

		/// <summary>
		/// The member ID string.
		/// </summary>
		public readonly string Id;

		/// <summary>
		/// Get the element specified by the given name.
		/// </summary>
		/// <param name="elementName">The name of the XML element to get.</param>
		/// <returns>The first element with the given name.  Returns null if the element does not exist.</returns>
		public XElement GetElement(string elementName)
		{
			Contract.Requires(elementName != null);

			return XElement.Element(elementName);
		}

		/// <summary>
		/// Get the "summary" text.
		/// </summary>
		public string GetSummary()
		{
			var summary = XElement.Element("summary");
			return summary != null ? summary.Value : "";
		}

		public void WriteElement(string elementName, XmlDocWriter writer, EmitOutputContext output)
		{
			Contract.Requires(writer != null);
			Contract.Requires(output != null);

			var element = GetElement(elementName);
			if (element == null)
				return;

			writer.Write(element, output);
		}

		/// <summary>
		/// The type of the member contained within this entry.  This is a single character from the beginning of the ID string.
		/// </summary>
		public char MemberType { get { return Id[0]; } }
	}

	public class DocumentSourcePolicy
	{
		private IEnumerable<string> EnumerateLines(string text)
		{
			Contract.Requires(text != null);

			var newlineChars = new char[] { '\r', '\n' };
			int prevIndex = 0;
			int index = text.IndexOfAny(newlineChars);
			while (index != -1)
			{
				// The end is one past the newline.
				++index;

				// Skip entire \r\n pairs.
				if (text[index - 1] == '\r' && index < text.Length && text[index] == '\n')
					++index;

				yield return text.Substring(prevIndex, index - prevIndex);

				prevIndex = index;

				// Find the next line ending.
				index = text.IndexOfAny(newlineChars, index);
			}

			// Return the last line in the string.
			yield return text.Substring(prevIndex);
		}

		public virtual XElement FixMember(XElement rawElement)
		{

			// Convert the element to text.
			var rawElementText = String.Join("", rawElement.Nodes());
			
			// Split into lines.
			var lines = EnumerateLines(rawElementText).ToArray();

			// We expect there to be at least 3 lines, if there aren't then don't bother trying to process this.
			if (lines.Length < 3)
				return rawElement;

			// There should only be whitespace in the first and last lines.  If not we don't process this.
			if(!string.IsNullOrWhiteSpace(lines[0]) || !string.IsNullOrWhiteSpace(lines[lines.Length- 1]))
				return rawElement;
			
			// Ignore the first and last lines since they enter and exit into the <member> element.
			var contentLines = lines.Skip(1).Take(lines.Length - 2);

			// Find the line with the least spaces.
			int minLeadingSpaces = int.MaxValue;
			foreach (var line in contentLines)
			{
				minLeadingSpaces = Math.Min(minLeadingSpaces, line.Length);
				for(int i = 0; i < line.Length && i < minLeadingSpaces; ++i)
				{
					if(line[i] != ' ')
					{
						minLeadingSpaces = i;
						break;
					}
				}
			}

			// Strip the spaces and recombine the strings.
			var resultString = string.Join("", contentLines.Select(line => line.Substring(minLeadingSpaces)));

			// Create a new element.
			var contentsElement = XElement.Parse("<root>" + resultString + "</root>", LoadOptions.PreserveWhitespace);
			var newElement = new XElement(rawElement);
			newElement.ReplaceNodes(contentsElement.Nodes());

			return newElement;
		}
	}

	/// <summary>
	/// This class contains the data and functionality for document source data.
	/// </summary>
	public class DocumentSource
	{
		/// <summary>
		/// Construct a DocumentSource.
		/// </summary>
		/// <param name="pathPairs">The assembly/XML path pairs for each file to be included in the documentation.</param>
		public DocumentSource(IEnumerable<XmlDocPathPair> pathPairs, DocumentSourcePolicy policy = null)
		{
			Contract.Requires(pathPairs != null);
			Contract.Requires(Contract.ForAll(pathPairs, a => a != null));

			// Create a default document source policy if one wasn't given.
			policy = policy ?? new DocumentSourcePolicy();
			m_policy = policy;

			// The builder for creating the member lookup.
			var lookupBuilder = ImmutableDictionary.CreateBuilder<DocumentSourceMemberEntryKey, DocumentSourceMemberEntry>();

			// Load each assembly and XML file.
			foreach (var pair in pathPairs)
			{
				var assembly = Assembly.LoadFrom(pair.AssemblyPath);
				var xmlDocument = XDocument.Load(pair.XmlDocPath, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

				// We don't want users of this data changing the document.
				xmlDocument.Changing += (sender, args) =>
				{
					throw new NotSupportedException("This XDocument cannot be modified.");
				};

				// Create an entry for each member.
				var memberEntries = xmlDocument.Root.Element("members").Elements()
					.Select(memberElement => new DocumentSourceMemberEntry(assembly, m_policy.FixMember(memberElement), memberElement, memberElement.Attribute("name").Value));

				// Add each of the entries to the lookup.
				lookupBuilder.AddRange(memberEntries.Select(entry =>
					new KeyValuePair<DocumentSourceMemberEntryKey, DocumentSourceMemberEntry>(
						new DocumentSourceMemberEntryKey(entry.Assembly, entry.Id),
						entry)));
			}

			// Set the lookup.
			m_entryLookup = lookupBuilder.ToImmutable();

			// Create the cached values.
			m_assemblies = new Lazy<IEnumerable<Assembly>>(() => MemberEntries.Select(entry => entry.Assembly).Distinct().ToList());
		}

		public virtual DocumentSourceMemberEntry TryGetEntry(MemberInfo memberInfo)
		{
			var assembly = memberInfo.Module.Assembly;
			var id = GetId(memberInfo);
			var key = new DocumentSourceMemberEntryKey(assembly, id);

			// If we can't get the value we return an empty document entry.
			DocumentSourceMemberEntry value;
			return m_entryLookup.TryGetValue(key, out value)
				? value
				: CreateEmptyEntry(assembly, id);
		}

		public virtual string GetId(MemberInfo memberInfo)
		{
			switch (memberInfo.MemberType)
			{
				case MemberTypes.Event:
					return "E:";
				case MemberTypes.Field:
					return "F:";
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					return "M:";
				case MemberTypes.Property:
					return "P:";
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					return "T:" + GetFullyQualifiedName((Type)memberInfo);

				default:
					throw new ArgumentOutOfRangeException("memberInfo", "No known member prefix for given member info.");
			}
		}

		public virtual string GetFullyQualifiedName(Type type)
		{
			// This starts with the namespace if we have no declaring type.
			var prefix = type.DeclaringType == null
				? type.Namespace
				: GetFullyQualifiedName(type.DeclaringType);

			if (prefix.Length > 0)
				return prefix + "." + EscapeName(type.Name);
			else
				return EscapeName(type.Name);
		}

		public virtual string EscapeName(string name)
		{
			// The XML documentation will change all '.' characters to '#'.
			return name.Replace('.', '#');
		}

		public virtual DocumentSourceMemberEntry CreateEmptyEntry(Assembly assembly, string id)
		{
			// Create an empty XElement with the 'name' attribute set.
			var element = new XElement("member");
			element.SetAttributeValue("name", id);

			return new DocumentSourceMemberEntry(assembly, element, element, id);
		}

		/// <summary>
		/// Return the assemblies contained within this document source.
		/// </summary>
		public IEnumerable<Assembly> Assemblies { get { return m_assemblies.Value; } }

		/// <summary>
		/// Gets an enumerable over all of the member entries.
		/// </summary>
		public IEnumerable<DocumentSourceMemberEntry> MemberEntries { get { return m_entryLookup.Values; } }

		/// <summary>
		/// The policy that governs this document source.
		/// </summary>
		private readonly DocumentSourcePolicy m_policy;

		/// <summary>
		/// A lookup of member ID to DocumentSourceMemberEntry.
		/// </summary>
		private readonly ImmutableDictionary<DocumentSourceMemberEntryKey, DocumentSourceMemberEntry> m_entryLookup;

		/// <summary>
		/// The cached assemblies list.
		/// </summary>
		private readonly Lazy<IEnumerable<Assembly>> m_assemblies;
	}
}
