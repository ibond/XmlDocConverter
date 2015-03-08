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
		public abstract Func<EmitContext<FinalContextType>, EmitContext> DefaultWriter { get; }
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

		public override Func<EmitContext<DocumentContextCollection<TDocContext>>, EmitContext> DefaultWriter
		{
			get 
			{
				return context =>
					{
						foreach (var element in m_elements)
						{
							//context.Write();
						}

						return context;
					};
			}
		}

		public IEnumerable<TDocContext> Elements { get { return m_elements; } }

		private readonly IEnumerable<TDocContext> m_elements;
	}

	
	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static partial class DocumentContextWriterExtensions
	{
		public class DocumentContextWriteExtensionImpl<TDocContext>
			where TDocContext : DocumentContext
		{
			public delegate EmitContext WriteDelegate(EmitContext<TDocContext> context);
			
			public static readonly object DataKey = new object();
		}

		public static EmitContext<TDocContext, TParentContext>
			Write<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext<TDocContext>
			where TParentContext : EmitContext
		{
			var writer = context.GetLocalData(
				DocumentContextWriteExtensionImpl<TDocContext>.DataKey, 
				context.GetDocumentContext().DefaultWriter);

			return writer(context)
				.ReplaceDocumentContext(context.GetDocumentContext())
				.ReplaceParentContext(context.GetParentContext());
		}

		public static EmitContext<TDocContext, TParentContext>
			Using<TDocContext, TParentContext, ReplaceDocumentContextType>(
				this EmitContext<TDocContext, TParentContext> context,
				DocumentContextWriteExtensionImpl<ReplaceDocumentContextType>.WriteDelegate writer)
			where TDocContext : DocumentContext<TDocContext>
			where TParentContext : EmitContext
			where ReplaceDocumentContextType : DocumentContext<TDocContext>
		{
			Contract.Requires(context != null);
			Contract.Requires(writer != null);

			// If writer is null we remove the value from the map.  This will cause us to return to using the default
			// writer whenever the Write function is called.
			return writer != null
				? context.UpdateLocalDataMap(map => map.SetItem(DocumentContextWriteExtensionImpl<ReplaceDocumentContextType>.DataKey, writer))
				: context.UpdateLocalDataMap(map => map.Remove(DocumentContextWriteExtensionImpl<ReplaceDocumentContextType>.DataKey));
		}
	}
}
