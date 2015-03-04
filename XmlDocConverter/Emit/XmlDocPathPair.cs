using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// A pair of paths for accessing XML documentation.
	/// </summary>
	public class XmlDocPathPair
	{
		/// <summary>
		/// Construct an XmlDocPathPair.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly.</param>
		/// <param name="xmlDocPath">The path to the XML document.  If null or whitespace this will use the assembly path with the extension changed to .xml.</param>
		public XmlDocPathPair(string assemblyPath, string xmlDocPath = null)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(assemblyPath));

			AssemblyPath = assemblyPath;

			// Find the XML file automatically if it's not specified.
			XmlDocPath = !String.IsNullOrWhiteSpace(xmlDocPath)
				? xmlDocPath
				: Path.ChangeExtension(AssemblyPath, "xml");
		}

		/// <summary>
		/// The path to the assembly file.
		/// </summary>
		public readonly string AssemblyPath;

		/// <summary>
		/// The path to the XML document file.
		/// </summary>
		public readonly string XmlDocPath;
	}
}
