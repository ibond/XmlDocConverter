﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This contains implementations for commonly used targets.
	/// </summary>
	public static class StandardTargets
	{
		/// <summary>
		/// Set the emit target to a TextWriter.
		/// </summary>
		/// <typeparam name="TDoc">The type of the document context used by this EmitContext.</typeparam>
		/// <param name="context">The emit context.</param>
		/// <param name="textWriter">The TextWriter to act as the target.</param>
		/// <returns>An updated emit context with the target set to this TextWriter.</returns>
		public static EmitContext<TDoc> ToTextWriter<TDoc>(this EmitContext<TDoc> context, TextWriter textWriter)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(textWriter != null);
			Contract.Requires(Contract.Result<EmitContext<TDoc>>() != null);
						
			// Replace the target context
			return context.ReplaceTargetContext(
				textWriter,
				() => new EmitTargetContext(
					new EmitOutputContext(),
					dataSources =>
					{
						// Write all of the sources to the TextWriter.
						foreach (var source in dataSources)
							textWriter.Write(source);
					}));
		}


		/// <summary>
		/// Set the emitter text writer to a file.  If this path does not have a path root it will be relative to the
		/// base directory.  If the base directory is not set it will be relative to the working directory.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="filePath">The path to the file.</param>
		/// <returns>An updated emit context with the target set to this file.</returns>
		public static EmitContext<TDoc> ToFile<TDoc>(this EmitContext<TDoc> context, string filePath)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(!String.IsNullOrWhiteSpace(filePath));
			Contract.Requires(Contract.Result<EmitContext<TDoc>>() != null);

			// Get the base directory.
			object baseDirectoryObject;
			string baseDirectory = context.GetLocalDataMap().TryGetValue(BaseDirectoryDataMapKey, out baseDirectoryObject)
				? (string)baseDirectoryObject
				: null;

			// Get the file path.
			var fullFilePath = Path.GetFullPath(
				Path.IsPathRooted(filePath) || String.IsNullOrWhiteSpace(baseDirectory)
					? filePath
					: Path.Combine(baseDirectory, filePath));
			
			// Return the updated context.
			return context.ReplaceTargetContext(
				FilePathDataMapKeys[fullFilePath],
				() => new EmitTargetContext(
					new EmitOutputContext(),
					dataSources =>
					{
						// Create the directory.
						Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));
						
						// Write all of the data.
						using (var writer = new StreamWriter(File.Open(fullFilePath, FileMode.Create, FileAccess.Write)))
						{
							// Replace newlines with the native version.							
							foreach (var source in dataSources.Select(s => s.Replace("\n", Environment.NewLine)))
								writer.Write(source);
						}
					}));
		}

		/// <summary>
		/// Set the base directory for where we are emitting files.
		/// </summary>
		/// <param name="directoryPath">The path to the base directory.</param>
		/// <returns>A new context with the updated base directory.</returns>
		public static EmitContext<TDoc> InDirectory<TDoc>(this EmitContext<TDoc> context, string directoryPath)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(!String.IsNullOrWhiteSpace(directoryPath));
			Contract.Requires(Contract.Result<EmitContext<TDoc>>() != null);

			// Set the base directory.
			return context.UpdateLocalDataMap(map => map.SetItem(BaseDirectoryDataMapKey, directoryPath));
		}
		
		/// <summary>
		/// Pass file paths through a unique key map to make sure equal strings that happen to be different objects
		/// still map to the same output context.
		/// </summary>
		private static Util.UniqueKeyMap<string> FilePathDataMapKeys = new Util.UniqueKeyMap<string>();

		/// <summary>
		/// This object is used as a key into the emit context data map for storing the base directory.
		/// </summary>
		private static object BaseDirectoryDataMapKey = new object();


		/// <summary>
		/// Set the emitter text writer so it writes to a string builder.
		/// </summary>
		/// <param name="stringBuilderTarget">The string builder that will accept the emitted text.</param>
		/// <returns>An updated emit context with the target set to this StringBuilder.</returns>
		public static EmitContext<TDoc> ToTextWriter<TDoc>(this EmitContext<TDoc> context, StringBuilder stringBuilderTarget)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(stringBuilderTarget != null);
			Contract.Requires(Contract.Result<EmitContext<TDoc>>() != null);

			// Replace the target context
			return context.ReplaceTargetContext(
				stringBuilderTarget,
				() => new EmitTargetContext(
					new EmitOutputContext(),
					dataSources =>
					{
						// Write all of the sources to the TextWriter.
						foreach (var source in dataSources)
							stringBuilderTarget.Append(source);
					}));
		}
	}
}
