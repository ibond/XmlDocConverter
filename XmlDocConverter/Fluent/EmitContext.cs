using NuDoq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.
	/// </summary>
	public struct EmitContext<ItemType>
		where ItemType : EmitItem
	{
		/// <summary>
		/// Construct a full EmitContext.
		/// </summary>
		/// <param name="emitItem">The emit item for this context.  May not be null.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		private EmitContext(
			ItemType emitItem, 
			EmitWriterContext emitWriter, 
			string baseDirectory,
			TextWriter textWriter)
		{
			Contract.Requires(emitItem != null);

			m_emitItem = emitItem;
			m_emitWriter = emitWriter;
			m_baseDirectory = baseDirectory;
			m_textWriter = textWriter;
		}

		/// <summary>
		/// Make a new EmitContext with a new EmitItem type based on an existing context.  Any unspecified parameters
		/// will be copied from the previous context.
		/// </summary>
		/// <typeparam name="NewItemType">The EmitItem type for the new context.</typeparam>
		/// <param name="prev">The previous context.</param>
		/// <param name="emitItem">The emit item for this context.  May not be null.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context.</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		/// <returns>A new EmitContext with updated values.</returns>
		public static EmitContext<NewItemType> MakeContext<NewItemType>(
			EmitContext<ItemType> prev,
			NewItemType emitItem, 
			EmitWriterContext emitWriter = null,
			string baseDirectory = null,
			TextWriter textWriter = null)
			where NewItemType : EmitItem
		{
			Contract.Requires(emitItem != null);

			return new EmitContext<NewItemType>(
				emitItem,
				Sel(emitWriter, prev.m_emitWriter),
				Sel(baseDirectory, prev.m_baseDirectory),
				Sel(textWriter, prev.m_textWriter));
		}

		/// <summary>
		/// Make a new EmitContext with the same EmitItem type based on an existing context.  Any unspecified parameters
		/// will be copied from the previous context.
		/// </summary>
		/// <param name="prev">The previous context.</param>
		/// <param name="emitItem">The emit item for this context.</param>
		/// <param name="documentSources">The assembly member sources to be used for this context.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context.</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		/// <returns>A new EmitContext with updated values.</returns>
		public static EmitContext<ItemType> MakeContext(
			EmitContext<ItemType> prev,
			ItemType emitItem = null,
			ImmutableList<AssemblyMembers> documentSources = null,
			EmitWriterContext emitWriter = null,
			string baseDirectory = null,
			TextWriter textWriter = null)
		{
			// We must always have an emit item.
			Contract.Requires(emitItem != null || prev.m_emitItem != null);

			return MakeContext(
				prev,
				Sel(emitItem, prev.m_emitItem),
				documentSources,
				emitWriter,
				baseDirectory,
				textWriter);
		}

		/// <summary>
		/// Helper function to update values if the new value isn't null.
		/// </summary>
		/// <typeparam name="T">The type of the value to be selected.</typeparam>
		/// <param name="newValue">The new value.</param>
		/// <param name="existingValue">The existing value.</param>
		/// <returns>newValue if newValue is not null, otherwise existingValue.</returns>
		private static T Sel<T>(T newValue, T existingValue) 
			where T : class
		{
			return newValue != null ? newValue : existingValue;
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <returns>A new context with the updated assembly.</returns>
		public EmitContext<DocSourceItem> From(string assemblyPath)
		{
			return From(new XmlDocPathPair[] { new XmlDocPathPair(assemblyPath, null) });
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <param name="xmlDocPath">The path to the XML document for the assembly.</param>
		/// <returns>A new context with the updated assembly.</returns>
		public EmitContext<DocSourceItem> From(string assemblyPath, string xmlDocPath)
		{
			return From(new XmlDocPathPair[] { new XmlDocPathPair(assemblyPath, xmlDocPath) });
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="assemblyPaths">The path to the assemblies whose documentation should be converted.</param>
		/// <returns>A new context with the updated assemblies.</returns>
		public EmitContext<DocSourceItem> From(IEnumerable<string> assemblyPaths)
		{
			return From(assemblyPaths.Select(assemblyPath => new XmlDocPathPair(assemblyPath)));
		}
		
		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="pathPairs">The pairs of document paths for each assembly and it's corresponding XML documentation.</param>
		/// <returns>A new context with the updated assemblies.</returns>
		public EmitContext<DocSourceItem> From(IEnumerable<XmlDocPathPair> pathPairs)
		{
			Contract.Requires(pathPairs != null);
			Contract.Requires(Contract.ForAll(pathPairs, p => p != null));

			// Load the documents and pass them to the doc source.
			var documentSource = pathPairs
				.Select(pair => DocReader.Read(Assembly.ReflectionOnlyLoadFrom(pair.AssemblyPath), pair.XmlDocPath))
				.ToImmutableList();

			return MakeContext<DocSourceItem>(this, new DocSourceItem(documentSource));
		}
		
		/// <summary>
		/// Construct a new EmitContext with the new writer.
		/// </summary>
		/// <param name="writer">The writer to be used for this context.</param>
		/// <returns>A new context with the updated writer.</returns>
		public EmitContext<ItemType> Using(EmitWriterContext writer)
		{
			Contract.Requires(writer != null);
			Contract.Ensures(this.m_emitWriter != null);

			return MakeContext(this, emitWriter: writer);
		}

		/// <summary>
		/// Set the base directory for where we are emitting files.
		/// </summary>
		/// <param name="directoryPath">The path to the base directory.</param>
		/// <returns>A new context with the updated base directory.</returns>
		public EmitContext<ItemType> InDirectory(string directoryPath)
		{
			Contract.Requires(directoryPath != null);

			return MakeContext(this, baseDirectory: directoryPath);
		}

		/// <summary>
		/// Set the emitter text writer.
		/// </summary>
		/// <param name="textWriter">The new text writer to which we should emit.</param>
		/// <returns>A new context with the text writer set.</returns>
		public EmitContext<ItemType> ToTextWriter(TextWriter textWriter)
		{
			Contract.Requires(textWriter != null);

			// Add the text writer to the run context so it get's dispose of when we're done.
			Script.CurrentRunContext.AddAutoDisposeObject(textWriter);

			return MakeContext(this, textWriter: textWriter);
		}

		/// <summary>
		/// Set the emitter text writer to a file.  If this path does not have a path root it will be relative to the
		/// base directory.  If the base directory is not set it will be relative to the working directory.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		/// <returns>A new context with the file path set.</returns>
		public EmitContext<ItemType> ToFile(string filePath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(filePath));
			
			// Get the file path.
			var fullFilePath = Path.GetFullPath(
				Path.IsPathRooted(filePath) || String.IsNullOrWhiteSpace(this.m_baseDirectory)
					? filePath
					: Path.Combine(this.m_baseDirectory, filePath));

			// Get the existing writer from the file write streams cache, or create a new one if it doesn't yet exist.
			var streamWriter = Script.CurrentRunContext.FileWriteStreams.GetOrAdd(
				fullFilePath,
				path =>
				{
					// Create the directory.
					Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));

					// Create the stream writer.
					return new StreamWriter(File.Open(fullFilePath, FileMode.Create, FileAccess.Write));
				});


			// Create the file writer.
			return ToTextWriter(streamWriter);
		}

		/// <summary>
		/// Set the emitter text writer so it writes to a string builder.
		/// </summary>
		/// <param name="stringBuilderTarget">The string builder that will accept the emitted text.</param>
		/// <returns>A new context with the text writer set.</returns>
		public EmitContext<ItemType> ToString(StringBuilder stringBuilderTarget)
		{
			Contract.Requires(stringBuilderTarget != null);

			return ToTextWriter(new StringWriter(stringBuilderTarget));
		}

		/// <summary>
		/// Executes the emit operations in scopeAction then restores the EmitContext to it's original state.
		/// </summary>
		/// <param name="scopeAction">The action containing the scoped emit operations.</param>
		/// <returns>The EmitContext as it was before Scope was called.</returns>
		public EmitContext<ItemType> Scope(Action<EmitContext<ItemType>> scopeAction)
		{
			Contract.Requires(scopeAction != null);

			// Pass a new emit context into the scope action.
			scopeAction(MakeContext(this));

			return this;
		}


		#region Write Methods
		// =====================================================================

		public EmitContext<ItemType> Write(AssemblyItem item)
		{
			//EmitWriter.Write(this, item);

			return this;
		}

		// =====================================================================
		#endregion


		/// <summary>
		/// Object.ToString() is not available for this object and will always throw an InvalidOperationException.
		/// </summary>
		/// <returns>Never returns, always throws an exception.</returns>
		public override string ToString()
		{
			throw new InvalidOperationException("ToString is not allowed for EmitContext.");
		}

		/// <summary>
		/// Gets the emit writer.  Throws an exception if the emit writer is not currently set.
		/// </summary>
		//private IEmitWriter EmitWriter
		//{
		//	get
		//	{
		//		if (m_emitWriter == null)
		//			throw new InvalidOperationException("Emit writer has not been set.  Set text writer using Using().");

		//		return m_emitWriter;
		//	}
		//}

		/// <summary>
		/// Get the emit item.
		/// </summary>
		public ItemType Get
		{
			get
			{
				if (m_emitItem == null)
					throw new InvalidOperationException("EmitItem has not been set.  This usually means you have not yet called From().");

				return m_emitItem;
			}
		}

		/// <summary>
		/// Gets the text writer.  Throws an exception if the text writer is not currently set.
		/// </summary>
		public TextWriter TextWriter
		{
			get
			{
				if(m_textWriter == null)
					throw new InvalidOperationException("Text writer has not been set.  Set text writer using ToTextWriter(), ToFile(), etc.");

				return m_textWriter;
			}
		}

		/// <summary>
		/// The emit writer for this emit context.
		/// </summary>
		private readonly EmitWriterContext m_emitWriter;

		/// <summary>
		/// The text writer target for emitted text.
		/// </summary>
		private readonly TextWriter m_textWriter;

		/// <summary>
		/// The base directory for where we emit files.
		/// </summary>
		private readonly string m_baseDirectory;

		/// <summary>
		/// The emit item for this context.
		/// </summary>
		private readonly ItemType m_emitItem;
	}
}
