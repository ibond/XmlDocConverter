using NuDoq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;

namespace XmlDocConverter
{
	/// <summary>
	/// This is the root class for the fluent emitter interface.
	/// </summary>
	public static class Emit
	{
		/// <summary>
		/// Start the fluent emit process with the given doc source.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <returns>A new setup with the updated assembly.</returns>
		public static EmitContext<DocSourceItem> From(string assemblyPath)
		{
			return new EmitContext<DocSourceItem>().From(assemblyPath);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc source.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <param name="xmlDocPath">The path to the XML document for the assembly.</param>
		/// <returns>A new setup with the updated assembly.</returns>
		public static EmitContext<DocSourceItem> From(string assemblyPath, string xmlDocPath)
		{
			return new EmitContext<DocSourceItem>().From(assemblyPath, xmlDocPath);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc sources.
		/// </summary>
		/// <param name="assemblyPaths">The path to the assemblies whose documentation should be converted.</param>
		/// <returns>A new setup with the updated assemblies.</returns>
		public static EmitContext<DocSourceItem> From(IEnumerable<string> assemblyPaths)
		{
			return new EmitContext<DocSourceItem>().From(assemblyPaths);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc sources.
		/// </summary>
		/// <param name="pathPairs">The pairs of document paths for each assembly and it's corresponding XML documentation.</param>
		/// <returns>A new setup with the updated assemblies.</returns>
		public static EmitContext<DocSourceItem> From(IEnumerable<XmlDocPathPair> pathPairs)
		{
			return new EmitContext<DocSourceItem>().From(pathPairs);
		}
	}
}
