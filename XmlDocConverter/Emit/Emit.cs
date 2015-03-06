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
			return new EmitContext().From(assemblyPath);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc source.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <param name="xmlDocPath">The path to the XML document for the assembly.</param>
		/// <returns>A new setup with the updated assembly.</returns>
		public static EmitContext<DocSourceItem> From(string assemblyPath, string xmlDocPath)
		{
			return new EmitContext().From(assemblyPath, xmlDocPath);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc sources.
		/// </summary>
		/// <param name="assemblyPaths">The path to the assemblies whose documentation should be converted.</param>
		/// <returns>A new setup with the updated assemblies.</returns>
		public static EmitContext<DocSourceItem> From(IEnumerable<string> assemblyPaths)
		{
			return new EmitContext().From(assemblyPaths);
		}

		/// <summary>
		/// Start the fluent emit process with the given doc sources.
		/// </summary>
		/// <param name="pathPairs">The pairs of document paths for each assembly and it's corresponding XML documentation.</param>
		/// <returns>A new setup with the updated assemblies.</returns>
		public static EmitContext<DocSourceItem> From(IEnumerable<XmlDocPathPair> pathPairs)
		{
			return new EmitContext().From(pathPairs);
		}

		/// <summary>
		/// Start the fluent emit process with the given emit writer.
		/// </summary>
		/// <param name="writer">The writer to be used for this context.</param>
		/// <returns>A new context with the updated writer.</returns>
		public static EmitContext Using(EmitWriterContext writer)
		{
			return new EmitContext().Using(writer);
		}

		/// <summary>
		/// Start the fluent emit process with the given directory path.
		/// </summary>
		/// <param name="directoryPath">The path to the base directory.</param>
		/// <returns>A new context with the updated base directory.</returns>
		public static EmitContext InDirectory(string directoryPath)
		{
			return new EmitContext().InDirectory(directoryPath);
		}

		/// <summary>
		/// Start the fluent emit process with the given text writer.
		/// </summary>
		/// <param name="textWriter">The new text writer to which we should emit.</param>
		/// <returns>A new context with the text writer set.</returns>
		public static EmitContext ToTextWriter(TextWriter textWriter)
		{
			return new EmitContext().ToTextWriter(textWriter);
		}

		/// <summary>
		/// Start the fluent emit process with the given file writer.  If this path does not have a path root it will be
		/// relative to the base directory.  If the base directory is not set it will be relative to the working
		/// directory.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		/// <returns>A new context with the file path set.</returns>
		public static EmitContext ToFile(string filePath)
		{
			return new EmitContext().ToFile(filePath);
		}

		/// <summary>
		/// Start the fluent emit process with the given string writer.
		/// </summary>
		/// <param name="stringBuilderTarget">The string builder that will accept the emitted text.</param>
		/// <returns>A new context with the text writer set.</returns>
		public static EmitContext ToString(StringBuilder stringBuilderTarget)
		{
			return new EmitContext().ToString(stringBuilderTarget);
		}
	}
}
