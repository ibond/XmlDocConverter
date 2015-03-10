using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public class EmitWriterItem<TDoc>
		where TDoc : DocumentContext<TDoc>
	{
		public EmitWriterItem(EmitContext<TDoc> emitContext)
		{
			Emit = emitContext;
			Document = emitContext.GetDocumentContext();
		}

		public readonly EmitContext<TDoc> Emit;
		public readonly TDoc Document;
	}

	/// <summary>
	/// A wrapper class for specifying an emit writer for a particular document context.
	/// </summary>
	/// <typeparam name="TDoc">The document context type that can be written with this writer.</typeparam>
	public class EmitWriter<TDoc>
		where TDoc : DocumentContext<TDoc>
	{
		/// <summary>
		/// The delegate type for writing this document context type.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="doc">The document context to be written.</param>
		/// <returns>An updated emit context.</returns>
		public delegate TemplateWriter Writer(EmitWriterItem<TDoc> writerItem);

		/// <summary>
		/// The key object for storing an emit writer in an emit context data map.
		/// </summary>
		public static readonly object DataKey = new object();


		/// <summary>
		/// Construct an emit writer.
		/// </summary>
		/// <param name="writer">The emit writer function.</param>
		public EmitWriter(Writer writer)
		{
			WriterFunction = writer;
		}

		/// <summary>
		/// The emit writer function.
		/// </summary>
		public readonly Writer WriterFunction;
	}


	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static class DocumentContextWriterExtensions
	{
		/// <summary>
		/// Write the document context contained within the context.
		/// </summary>
		/// <typeparam name="TDoc">The type of the document context.</typeparam>
		/// <typeparam name="TParent">The type of the emit context parent.</typeparam>
		/// <param name="context">The current context.</param>
		/// <returns>An updated context.</returns>
		public static TParent Write<TDoc, TParent>(this EmitContext<TDoc, TParent> context)
			where TDoc : DocumentContext<TDoc>
			where TParent : EmitContext
		{
			// Get the writer function.
			var writer = context.GetLocalData(
				EmitWriter<TDoc>.DataKey,
				context.GetDocumentContext().DefaultWriter);

			// Create a new output context for the results of this write and insert it in the current context.
			var subContext = context.ReplaceOutputContext(new EmitOutputContext());

			// The writer function will return a TemplateWriter object.
			var templateWriter = writer(new EmitWriterItem<TDoc>(subContext));
			
			// Write to the subcontext's output context.
			templateWriter.WriteTo(subContext.GetOutputContext());

			// Append the results of the subcontext write.
			context.GetOutputContext().Write(subContext.GetOutputContext());

			// Return the parent context.
			return context.GetParentContext();
		}


		/// <summary>
		/// Change the emit writer used for the given document context type.
		/// </summary>
		/// <typeparam name="TDoc">The type of the document context for the current emit context.</typeparam>
		/// <typeparam name="TParent">The type of the emit context parent.</typeparam>
		/// <typeparam name="TUsingDoc">The type of the document type to be replaced in the context.</typeparam>
		/// <param name="context">The current context.</param>
		/// <param name="writer">The new document context writer.  If this is null the default writer will be restored.</param>
		/// <returns>An updated context.</returns>
		public static EmitContext<TDoc, TParent> Using<TDoc, TParent, TUsingDoc>(this EmitContext<TDoc, TParent> context, EmitWriter<TUsingDoc>.Writer writer)
			where TDoc : DocumentContext<TDoc>
			where TParent : EmitContext
			where TUsingDoc : DocumentContext<TUsingDoc>
		{
			Contract.Requires(context != null);
			Contract.Requires(writer != null);

			// If writer is null we remove the value from the map.  This will cause us to return to using the default
			// writer whenever the Write function is called.
			return writer != null
				? context.UpdateLocalDataMap(map => map.SetItem(EmitWriter<TUsingDoc>.DataKey, writer))
				: context.UpdateLocalDataMap(map => map.Remove(EmitWriter<TUsingDoc>.DataKey));
		}


		/// <summary>
		/// Change the emit writer used for the given document context type.
		/// </summary>
		/// <typeparam name="TDoc">The type of the document context for the current emit context.</typeparam>
		/// <typeparam name="TParent">The type of the emit context parent.</typeparam>
		/// <typeparam name="TUsingDoc">The type of the document type to be replaced in the context.</typeparam>
		/// <param name="context">The current context.</param>
		/// <param name="writer">The new document context writer object.</param>
		/// <returns>An updated context.</returns>
		public static EmitContext<TDoc, TParent>
			Using<TDoc, TParent, TUsingDoc>(
				this EmitContext<TDoc, TParent> context,
				EmitWriter<TUsingDoc> writer)
			where TDoc : DocumentContext<TDoc>
			where TParent : EmitContext
			where TUsingDoc : DocumentContext<TUsingDoc>
		{
			// Extract the writer function.
			return context.Using(writer.WriterFunction);
		}
	}
}
