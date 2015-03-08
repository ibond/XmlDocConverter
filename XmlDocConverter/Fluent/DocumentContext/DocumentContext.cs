using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public abstract class DocumentContext
	{
	}

	public abstract class DocumentContext<FinalContextType> : DocumentContext
		where FinalContextType : DocumentContext<FinalContextType>
	{
		public abstract EmitWriter<FinalContextType>.Writer DefaultWriter { get; }
	}

	public abstract class ScalarDocumentContext<FinalContextType> : DocumentContext<FinalContextType>
		where FinalContextType : ScalarDocumentContext<FinalContextType>
	{
		/// <summary>
		/// Construct a DocumentContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public ScalarDocumentContext(DocumentSource documentSource)
		{
			Contract.Requires(documentSource != null);
			Contract.Ensures(m_documentSource != null);

			m_documentSource = documentSource;
		}

		/// <summary>
		/// Gets the document source.
		/// </summary>
		public DocumentSource DocumentSource { get { return m_documentSource; } }

		/// <summary>
		/// The document source.
		/// </summary>
		private readonly DocumentSource m_documentSource;
	}

	public interface IDocumentContextCollection<out TDocContext>
	{
		IEnumerable<TDocContext> Elements { get; }
	}

	public class DocumentContextCollection<TDocContext> : DocumentContext<DocumentContextCollection<TDocContext>>, IDocumentContextCollection<TDocContext>
		where TDocContext : DocumentContext
	{
		public DocumentContextCollection(IEnumerable<TDocContext> elements)
		{
			m_elements = elements;
		}

		public override EmitWriter<DocumentContextCollection<TDocContext>>.Writer DefaultWriter
		{
			get 
			{
				return context =>
					{
						foreach (var element in m_elements)
						{
							context.Write();
						}

						return context;
					};
			}
		}

		public IEnumerable<TDocContext> Elements { get { return m_elements; } }

		private readonly IEnumerable<TDocContext> m_elements;
	}
	

	public class EmitWriter<TDocContext>
		where TDocContext : DocumentContext<TDocContext>
	{
		public EmitWriter(Writer writer)
		{
			WriterFunction = writer;
		}

		public readonly Writer WriterFunction;


		public delegate EmitContext Writer(EmitContext<TDocContext> context);

		public static readonly object DataKey = new object();
	}

	
	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static class DocumentContextWriterExtensions
	{
		public static EmitContext<TDocContext, TParentContext>
			Write<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext<TDocContext>
			where TParentContext : EmitContext
		{
			var writer = context.GetLocalData(
				EmitWriter<TDocContext>.DataKey, 
				context.GetDocumentContext().DefaultWriter);

			return writer(context)
				.ReplaceDocumentAndParentContext(context.GetDocumentContext(), context.GetParentContext());
		}

		public static EmitContext<TDocContext, TParentContext>
			Using<TDocContext, TParentContext, ReplaceDocumentContextType>(
				this EmitContext<TDocContext, TParentContext> context,
				EmitWriter<ReplaceDocumentContextType>.Writer writer)
			where TDocContext : DocumentContext<TDocContext>
			where TParentContext : EmitContext
			where ReplaceDocumentContextType : DocumentContext<ReplaceDocumentContextType>
		{
			Contract.Requires(context != null);
			Contract.Requires(writer != null);

			// If writer is null we remove the value from the map.  This will cause us to return to using the default
			// writer whenever the Write function is called.
			return writer != null
				? context.UpdateLocalDataMap(map => map.SetItem(EmitWriter<ReplaceDocumentContextType>.DataKey, writer))
				: context.UpdateLocalDataMap(map => map.Remove(EmitWriter<ReplaceDocumentContextType>.DataKey));
		}

		public static EmitContext<TDocContext, TParentContext>
			Using<TDocContext, TParentContext, ReplaceDocumentContextType>(
				this EmitContext<TDocContext, TParentContext> context,
				EmitWriter<ReplaceDocumentContextType> writer)
			where TDocContext : DocumentContext<TDocContext>
			where TParentContext : EmitContext
			where ReplaceDocumentContextType : DocumentContext<ReplaceDocumentContextType>
		{
			return context.Using(writer.WriterFunction);
		}
	}
}
