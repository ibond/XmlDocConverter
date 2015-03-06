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
	/// The base class for EmitContext.  This contains the data that does not depend on the specific item type of the
	/// context.
	/// </summary>
	public class EmitContext
	{
		/// <summary>
		/// Construct an empty EmitContext.  This is used to start a context chain.
		/// </summary>
		public EmitContext()
		{
		}

		/// <summary>
		/// Construct a full EmitContext.
		/// </summary>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		protected EmitContext(
			EmitWriterContext emitWriter,
			string baseDirectory,
			TextWriter textWriter)
		{
			m_emitWriter = emitWriter;
			m_baseDirectory = baseDirectory;
			m_textWriter = textWriter;
		}


		/// <summary>
		/// Construct a full EmitContext based on the values of a previous context.  Any null parameters are copied from
		/// prev.
		/// </summary>
		/// <param name="prev">The context that we should use as a base.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		protected EmitContext(
			EmitContext prev,
			EmitWriterContext emitWriter = null,
			string baseDirectory = null,
			TextWriter textWriter = null)
		{	
			m_emitWriter = Sel(emitWriter, prev.m_emitWriter);
			m_baseDirectory = Sel(baseDirectory, prev.m_baseDirectory);
			m_textWriter = Sel(textWriter, prev.m_textWriter);
		}

		/// <summary>
		/// Helper function to update values if the new value isn't null.
		/// </summary>
		/// <typeparam name="T">The type of the value to be selected.</typeparam>
		/// <param name="newValue">The new value.</param>
		/// <param name="existingValue">The existing value.</param>
		/// <returns>newValue if newValue is not null, otherwise existingValue.</returns>
		protected static T Sel<T>(T newValue, T existingValue)
			where T : class
		{
			return newValue != null ? newValue : existingValue;
		}


		#region Emit Setup Functions
		// =====================================================================

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

			return new EmitContext<DocSourceItem>(
				this,
				new LazyEmitItem<DocSourceItem>(context => new DocSourceItem(documentSource, context)));
		}

		/// <summary>
		/// Construct a new EmitContext with the new writer.
		/// </summary>
		/// <param name="writer">The writer to be used for this context.</param>
		/// <returns>A new context with the updated writer.</returns>
		public EmitContext Using(EmitWriterContext writer)
		{
			Contract.Requires(writer != null);
			Contract.Ensures(this.m_emitWriter != null);

			return new EmitContext(this, emitWriter: writer);
		}

		/// <summary>
		/// Set the base directory for where we are emitting files.
		/// </summary>
		/// <param name="directoryPath">The path to the base directory.</param>
		/// <returns>A new context with the updated base directory.</returns>
		public EmitContext InDirectory(string directoryPath)
		{
			Contract.Requires(directoryPath != null);

			return new EmitContext(this, baseDirectory: directoryPath);
		}

		/// <summary>
		/// Set the emitter text writer.
		/// </summary>
		/// <param name="textWriter">The new text writer to which we should emit.</param>
		/// <returns>A new context with the text writer set.</returns>
		public EmitContext ToTextWriter(TextWriter textWriter)
		{
			Contract.Requires(textWriter != null);

			// Add the text writer to the run context so it get's dispose of when we're done.
			Script.CurrentRunContext.AddAutoDisposeObject(textWriter);

			return new EmitContext(this, textWriter: textWriter);
		}

		/// <summary>
		/// Set the emitter text writer to a file.  If this path does not have a path root it will be relative to the
		/// base directory.  If the base directory is not set it will be relative to the working directory.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		/// <returns>A new context with the file path set.</returns>
		public EmitContext ToFile(string filePath)
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
		public EmitContext ToString(StringBuilder stringBuilderTarget)
		{
			Contract.Requires(stringBuilderTarget != null);

			return ToTextWriter(new StringWriter(stringBuilderTarget));
		}

		/// <summary>
		/// Executes the emit operations in scopeAction then restores the EmitContext to it's original state.
		/// </summary>
		/// <param name="scopeAction">The action containing the scoped emit operations.</param>
		/// <returns>The EmitContext as it was before Scope was called.</returns>
		public EmitContext Scope(Action<EmitContext> scopeAction)
		{
			Contract.Requires(scopeAction != null);

			// Pass a new emit context into the scope action.
			scopeAction(new EmitContext(this));

			return this;
		}

		// =====================================================================
		#endregion
		
		/// <summary>
		/// Gets the emit writer.  Throws an exception if the emit writer is not currently set.
		/// </summary>
		public EmitWriterContext EmitWriter
		{
			get
			{
				if (m_emitWriter == null)
					throw new InvalidOperationException("Emit writer has not been set.  Set emit writer using Using().");

				return m_emitWriter;
			}
		}

		/// <summary>
		/// Gets the text writer.  Throws an exception if the text writer is not currently set.
		/// </summary>
		public TextWriter TextWriter
		{
			get
			{
				if (m_textWriter == null)
					throw new InvalidOperationException("Text writer has not been set.  Set text writer using ToTextWriter(), ToFile(), etc.");

				return m_textWriter;
			}
		}

		/// <summary>
		/// Object.ToString() is not available for this object and will always throw an InvalidOperationException.
		/// </summary>
		/// <returns>Never returns, always throws an exception.</returns>
		public override string ToString()
		{
			throw new InvalidOperationException("ToString is not allowed for EmitContext.");
		}

		/// <summary>
		/// The emit writer for this emit context.
		/// </summary>
		protected readonly EmitWriterContext m_emitWriter;

		/// <summary>
		/// The text writer target for emitted text.
		/// </summary>
		protected readonly TextWriter m_textWriter;

		/// <summary>
		/// The base directory for where we emit files.
		/// </summary>
		protected readonly string m_baseDirectory;
	}

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.
	/// </summary>
	public class EmitContext<ItemType> : EmitContext
		where ItemType : EmitItem
	{
		/// <summary>
		/// Construct a full EmitContext.
		/// </summary>
		/// <param name="emitItem">The emit item for this context.  May not be null.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		public EmitContext(
			EmitContext prev,
			LazyEmitItem<ItemType> emitItem,
			EmitWriterContext emitWriter = null,
			string baseDirectory = null,
			TextWriter textWriter = null)
			:base(prev, emitWriter, baseDirectory, textWriter)
		{
			Contract.Requires(emitItem != null);
			Contract.Ensures(m_emitItem != null);
			
			m_emitItem = emitItem;
		}

		/// <summary>
		/// Construct a full EmitContext with EmitItem replacement.
		/// </summary>
		/// <param name="prev">The context that we should use as a base.</param>
		/// <param name="emitItem">The emit item for this context.</param>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		/// <param name="baseDirectory">The base directory to be used for this context</param>
		/// <param name="textWriter">The text writer to be used for this context.</param>
		private EmitContext(
			EmitContext<ItemType> prev,
			LazyEmitItem<ItemType> emitItem = null,
			EmitWriterContext emitWriter = null,
			string baseDirectory = null,
			TextWriter textWriter = null)
			: base(prev, emitWriter, baseDirectory, textWriter)
		{
			Contract.Requires(emitItem != null || prev.m_emitItem != null);
			Contract.Ensures(m_emitItem != null);

			m_emitItem = Sel(emitItem, prev.m_emitItem);
		}


		#region EmitContext Modification
		// =====================================================================

		/// <summary>
		/// Construct a new EmitContext with a new EmitWriter.
		/// </summary>
		/// <param name="emitWriter">The writer to be used for this context.</param>
		public EmitContext<ItemType> ReplaceEmitWriter(EmitWriterContext emitWriter)
		{
			Contract.Requires(emitWriter != null);

			return emitWriter != this.m_emitWriter
				? new EmitContext<ItemType>(this, emitWriter: emitWriter)
				: this;
		}

		// =====================================================================
		#endregion


		#region Emit Setup Functions
		// =====================================================================
		
		/// <summary>
		/// Construct a new EmitContext with the new writer.
		/// </summary>
		/// <param name="writer">The writer to be used for this context.</param>
		/// <returns>A new context with the updated writer.</returns>
		public new EmitContext<ItemType> Using(EmitWriterContext writer)
		{
			return new EmitContext<ItemType>(base.Using(writer), m_emitItem);
		}

		/// <summary>
		/// Set the base directory for where we are emitting files.
		/// </summary>
		/// <param name="directoryPath">The path to the base directory.</param>
		/// <returns>A new context with the updated base directory.</returns>
		public new EmitContext<ItemType> InDirectory(string directoryPath)
		{
			return new EmitContext<ItemType>(base.InDirectory(directoryPath), m_emitItem);
		}

		/// <summary>
		/// Set the emitter text writer.
		/// </summary>
		/// <param name="textWriter">The new text writer to which we should emit.</param>
		/// <returns>A new context with the text writer set.</returns>
		public new EmitContext<ItemType> ToTextWriter(TextWriter textWriter)
		{
			return new EmitContext<ItemType>(base.ToTextWriter(textWriter), m_emitItem);
		}

		/// <summary>
		/// Set the emitter text writer to a file.  If this path does not have a path root it will be relative to the
		/// base directory.  If the base directory is not set it will be relative to the working directory.
		/// </summary>
		/// <param name="filePath">The path to the file.</param>
		/// <returns>A new context with the file path set.</returns>
		public new EmitContext<ItemType> ToFile(string filePath)
		{
			return new EmitContext<ItemType>(base.ToFile(filePath), m_emitItem);
		}

		/// <summary>
		/// Set the emitter text writer so it writes to a string builder.
		/// </summary>
		/// <param name="stringBuilderTarget">The string builder that will accept the emitted text.</param>
		/// <returns>A new context with the text writer set.</returns>
		public new EmitContext<ItemType> ToString(StringBuilder stringBuilderTarget)
		{
			return new EmitContext<ItemType>(base.ToString(stringBuilderTarget), m_emitItem);
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
			scopeAction(new EmitContext<ItemType>(this));

			return this;
		}

		// =====================================================================
		#endregion


		#region Write Methods
		// =====================================================================

		/// <summary>
		/// Write the current emit context.
		/// </summary>
		/// <returns>A context that has been updated by the write.</returns>
		public EmitContext<ItemType> Write()
		{
			return (EmitContext<ItemType>)EmitWriter.Write(this);
		}

		// =====================================================================
		#endregion
		

		#region Emit Value Writer Methods
		// =====================================================================
		
		/// <summary>
		/// Write the given value to the text writer.
		/// </summary>
		/// <typeparam name="ValueType">The type of the value to be written.</typeparam>
		/// <param name="value">The value to be written.</param>
		/// <returns>This EmitContext for chaining.</returns>
		public EmitContext<ItemType> Write<ValueType>(ValueType value)
		{
			TextWriter.Write(value); 
			return this;
		}

		// =====================================================================
		#endregion

		/// <summary>
		/// Get the emit item.  Same as Item, but with fluent naming.
		/// </summary>
		public ItemType Get { get { return Item; } }

		/// <summary>
		/// Get the emit item.
		/// </summary>
		public ItemType Item
		{
			get
			{
				if (m_emitItem == null)
					throw new InvalidOperationException("EmitItem has not been set.  This usually means you have not yet called From().");

				return m_emitItem.GetItem(this);
			}
		}

		/// <summary>
		/// The emit item in a lazy wrapper.
		/// </summary>
		private readonly LazyEmitItem<ItemType> m_emitItem;
	}
}
