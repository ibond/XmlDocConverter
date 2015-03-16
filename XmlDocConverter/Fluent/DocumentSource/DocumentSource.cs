using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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
		/// <returns>The first element with the given name.</returns>
		public XElement GetElement(string elementName)
		{
			Contract.Requires(elementName != null);

			return XElement.Element(elementName) ?? new XElement(elementName);
		}

		/// <summary>
		/// Get the "summary" text.
		/// </summary>
		public string GetSummary()
		{
			var summary = XElement.Element("summary");
			return summary != null ? summary.Value : "";
		}

		public EmitContextX WriteElement(string elementName, XmlDocWriter writer, EmitContextX context)
		{
			Contract.Requires(writer != null);
			Contract.Requires(context != null);

			var element = GetElement(elementName);
			if (element == null)
				return context;

			return writer.Write(element, context);
		}

		/// <summary>
		/// The type of the member contained within this entry.  This is a single character from the beginning of the ID string.
		/// </summary>
		public char MemberType { get { return Id[0]; } }
	}

	public class DocumentSourcePolicy
	{
		private string GetContentsAsString(XElement element)
		{
			var reader = element.CreateReader();
			reader.MoveToContent();
			return reader.ReadInnerXml();
		}

		private static readonly char[] NonNewlineWhitespaceChars = Enumerable.Range(0, ((int)char.MaxValue) + 1)
			.Select(i => (char)i)
			.Where(c => c != '\n' && Char.IsWhiteSpace(c))
			.ToArray();
		public virtual string TrimWhitespaceLines(string content)
		{
			var trimmed = content.Trim(NonNewlineWhitespaceChars);
			if (trimmed.Length == 0 || (trimmed.Length == 1 && trimmed[0] == '\n'))
				return String.Empty;

			// Remove the leading and trailing newline.
			int substringStart = trimmed[0] == '\n' ? 1 : 0;
			int substringLength = trimmed.Length - substringStart -
				(trimmed[trimmed.Length - 1] == '\n' ? 1 : 0);

			return trimmed.Substring(substringStart, substringLength);
		}

		public virtual XElement PreprocessMember(XElement rawElement)
		{
			// Copy the element so we can modify it.
			var elementCopy = new XElement(rawElement);

			// Get the contents of this element as a string so we can unindent it.
			var contents = GetContentsAsString(elementCopy);

			// Trim leading and trailing whitespace lines.
			var trimmedContents = TrimWhitespaceLines(contents);
						
			// Split into lines.
			var lines = trimmedContents.Split(new char[] { '\n' });
			
			// Find the line with the least spaces.
			int minLeadingSpaces = int.MaxValue;
			foreach (var line in lines)
			{
				minLeadingSpaces = Math.Min(minLeadingSpaces, line.Length);
				for (int i = 0; i < line.Length && i < minLeadingSpaces; ++i)
				{
					if (line[i] != ' ')
					{
						minLeadingSpaces = i;
						break;
					}
				}
			}

			// Strip the spaces and recombine the strings.
			var resultString = string.Join("\n", lines.Select(line => line.Substring(minLeadingSpaces)));

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
					.Select(memberElement => new DocumentSourceMemberEntry(assembly, m_policy.PreprocessMember(memberElement), memberElement, memberElement.Attribute("name").Value));

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
					return GetIdString((EventInfo)memberInfo);
				case MemberTypes.Field:
					return GetIdString((FieldInfo)memberInfo);
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					return GetIdString((MethodInfo)memberInfo);
				case MemberTypes.Property:
					return GetIdString((PropertyInfo)memberInfo);
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					return GetIdString((Type)memberInfo);

				default:
					throw new ArgumentOutOfRangeException("memberInfo", "No known member prefix for given member info.");
			}
		}

		public virtual string GetIdString(Type type)
		{
			return "T:" + GetFullyQualifiedName(type);
		}

		public virtual string GetIdString(EventInfo info)
		{
			// Events just return the fully qualified name.
			return "E:" + GetFullyQualifiedName(info);
		}

		public virtual string GetIdString(FieldInfo info)
		{
			// Fields just return the fully qualified name.
			return "F:" + GetFullyQualifiedName(info);
		}

		public virtual string GetIdString(PropertyInfo info)
		{
			// Get the fully qualified property name.
			var baseName = GetFullyQualifiedName(info);

			// Get the method arguments.  We look at the getter to see if there are parameters to the member (e.g. this
			// is an indexer).
			var parameters = GetMethodArguments(info.GetMethod);

			// Return the combined string.
			return "P:" + baseName + parameters;
		}

		public virtual string GetIdString(MethodInfo info)
		{
			// Get the fully qualified method name.
			var baseName = GetFullyQualifiedName(info);

			// Get the method arguments.
			var parameters = GetMethodArguments(info);

			// If this is a conversion operator we need to append the return type.
			var suffix = "";
			if (info.IsSpecialName && info.IsStatic && (info.Name == "op_Explicit" || info.Name == "op_Implicit"))
			{
				suffix = '~' + GetFullyQualifiedName(info.ReturnType);
			}

			// Return the combined string.
			return "M:" + baseName + parameters + suffix;
		}

		public virtual string GetFullyQualifiedName(MemberInfo info)
		{
			// Prefix with the name of the declaring type.
			return GetFullyQualifiedName(info.DeclaringType)
				+ "." + EscapeName(info.Name);
		}

		public virtual string GetFullyQualifiedName(PropertyInfo info)
		{
			// Prefix with the name of the declaring type.  
			return GetFullyQualifiedName(info.DeclaringType)
				+ "." + EscapeName(info.Name);
		}

		public virtual string GetMethodArguments(MethodInfo info)
		{
			// Get the parameters.
			var parameters = info.GetParameters();

			// If there are no parameters we return an empty string.
			if (parameters.Length == 0)
				return String.Empty;

			// Get the fully qualified name for each parameter.
			var formattedParameters = parameters.Select(parameter => GetFullyQualifiedName(parameter.ParameterType));

			// Return the parameter string.
			return '(' + String.Join(",", formattedParameters) +')';
		}

		public virtual string GetFullyQualifiedName(Type type)
		{
			// If this is a ref type we get it's element type and append an @ symbol.
			if (type.IsByRef)
				return GetFullyQualifiedName(type.GetElementType()) + '@';

			// If this is an array type we need to explicitly append bounds.
			if (type.IsArray)
			{
				// Get the name of the element type.
				var elementTypeName = GetFullyQualifiedName(type.GetElementType());

				// Get the rank of the array.
				var arrayRank = type.GetArrayRank();

				// A rank of 1 will just append an empty set of brackets, otherwise we must fill in the bounds.
				// According to the docs each array dimension will indicate it's lower bound and size, however that's
				// not information that's available at the type level and there doesn't seem to be any way to
				// distinguish between array bounds without looking at IL.  Since we can't do anything about it we just
				// use "0:".
				return elementTypeName + (arrayRank == 1
					? "[]"
					: '[' + String.Join(",", Enumerable.Repeat("0:", arrayRank)) + ']');
			}


			// This starts with the namespace if we have no declaring type.
			var prefix = type.DeclaringType == null
				? (type.Namespace ?? String.Empty)
				: GetFullyQualifiedName(type.DeclaringType);

			var typeName = EscapeName(type.Name);

			// If this is a generic type with the generic arguments filled in then we need to replace each argument in
			// the type name.
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				// Remove the number of arguments identifier (e.g. `1) from the end of the string.
				var baseTypeName = typeName.Substring(0, typeName.IndexOf('`'));

				// Build the type arguments.
				var arguments = type.GetGenericArguments();
				var formattedArguments = arguments.Select(arg => GetFullyQualifiedName(arg));

				// Build the concrete type name.
				typeName = baseTypeName + '{' + String.Join(",", formattedArguments) + '}';
			}

			if (prefix.Length > 0)
				return prefix + "." + typeName;
			else
				return typeName;
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
