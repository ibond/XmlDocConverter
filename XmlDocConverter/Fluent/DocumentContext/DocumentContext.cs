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

	public interface IDocumentContextCollection<out DocumentContextType>
	{
		IEnumerable<DocumentContextType> Elements { get; }
	}

	public class DocumentContextCollection<DocumentContextType> : DocumentContext<DocumentContextCollection<DocumentContextType>>, IDocumentContextCollection<DocumentContextType>
		where DocumentContextType : DocumentContext
	{
		public DocumentContextCollection(IEnumerable<DocumentContextType> elements)
		{
			m_elements = elements;
		}

		public override Func<EmitContext<DocumentContextCollection<DocumentContextType>>, EmitContext> DefaultWriter
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

		public IEnumerable<DocumentContextType> Elements { get { return m_elements; } }

		private readonly IEnumerable<DocumentContextType> m_elements;
	}

	
	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static partial class DocumentContextWriterExtensions
	{
		public class DocumentContextWriteExtensionImpl<DocumentContextType>
			where DocumentContextType : DocumentContext
		{
			public delegate EmitContext WriteDelegate(EmitContext<DocumentContextType> context);
			
			public static readonly object DataKey = new object();
		}

		public static EmitContext<DocumentContextType, ParentEmitContextType>
			Write<DocumentContextType, ParentEmitContextType>(
				this EmitContext<DocumentContextType, ParentEmitContextType> context)
			where DocumentContextType : DocumentContext<DocumentContextType>
			where ParentEmitContextType : EmitContext
		{
			var writer = context.GetLocalData(
				DocumentContextWriteExtensionImpl<DocumentContextType>.DataKey, 
				context.GetDocumentContext().DefaultWriter);

			return writer(context)
				.ReplaceDocumentContext(context.GetDocumentContext())
				.ReplaceParentContext(context.GetParentContext());
		}

		public static EmitContext<DocumentContextType, ParentEmitContextType>
			Using<DocumentContextType, ParentEmitContextType, ReplaceDocumentContextType>(
				this EmitContext<DocumentContextType, ParentEmitContextType> context,
				DocumentContextWriteExtensionImpl<ReplaceDocumentContextType>.WriteDelegate writer)
			where DocumentContextType : DocumentContext<DocumentContextType>
			where ParentEmitContextType : EmitContext
			where ReplaceDocumentContextType : DocumentContext<DocumentContextType>
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
