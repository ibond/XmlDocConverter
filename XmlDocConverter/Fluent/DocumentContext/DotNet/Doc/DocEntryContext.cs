using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlDocConverter.Fluent.Detail;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is a context representing a documentation entry which contains documentation data.
	/// </summary>
	public class DocEntryContext : DotNetDocumentContext<DocEntryContext>, DocElementContext.IProvider
	{
		/// <summary>
		/// Construct an DocEntryContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public DocEntryContext(DocumentSource documentSource, DocumentSourceMemberEntry entry)
			: base(documentSource)
		{
			Contract.Requires(entry != null);

			m_entry = entry;
		}

		protected override Action<EmitWriterItem<DocEntryContext>> GetDefaultRenderer()
		{
			return item =>
				{
					item.Emit.WriteXmlElement("summary");
				};
		}

		public DocElementContext GetDocElement(string elementName)
		{
			return new DocElementContext(DocumentSource, Entry.GetElement(elementName));
		}
		
		/// <summary>
		/// Access the entry directly.
		/// </summary>
		public DocumentSourceMemberEntry Entry { get { return m_entry; } }

		/// <summary>
		/// The document entry.
		/// </summary>
		private readonly DocumentSourceMemberEntry m_entry;

		/// <summary>
		/// The interface for getting a documentation entry.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get the documentation entry.
			/// </summary>
			DocEntryContext GetDocEntry();
		}
	}

	/// <summary>
	/// Selector extensions for DocEntryContext.
	/// </summary>
	public static class DocEntryContextProviderExtensions
	{
		/// <summary>
		/// Select all of the classes.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocEntryContext, EmitContext<TDoc, TParent>>

			Doc<TDoc, TParent>(this IContextSelector<TDoc, TParent, DocEntryContext.IProvider> selector)

			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocEntryContext, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentAndParentContext(
				selector.DocumentContext.GetDocEntry(),
				selector.EmitContext);
		}


		public static EmitContext<TDoc, TParent> Using<TDoc, TParent>(this EmitContext<TDoc, TParent> context, Func<IXmlDocWriter> docWriter)
			where TDoc : DocumentContext<TDoc>
			where TParent : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(docWriter != null);

			// If writer is null we remove the value from the map.  This will cause us to return to using the default
			// writer whenever the Write function is called.
			return docWriter != null
				? context.UpdateLocalDataMap(map => map.SetItem(XmlDocWriterKey, docWriter))
				: context.UpdateLocalDataMap(map => map.Remove(XmlDocWriterKey));
		}

		public static EmitContext<DocEntryContext, TParent> WriteXmlElement<TParent>(this EmitContext<DocEntryContext, TParent> context, string elementName)
			where TParent : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(elementName != null);

			context.GetDocumentContext().Entry.WriteElement(elementName, context.CreateXmlDocWriter(), context.GetOutputContext());

			return context;
		}

		public static EmitContext<DocElementContext, TParent> WriteXmlElement<TParent>(this EmitContext<DocElementContext, TParent> context)
			where TParent : EmitContext
		{
			Contract.Requires(context != null);
			
			context.CreateXmlDocWriter().Write(context.GetDocumentContext().Element, context.GetOutputContext());

			return context;
		}


		public static IXmlDocWriter CreateXmlDocWriter(this EmitContext context)
		{
			Contract.Requires(context != null);

			return context.GetLocalData(XmlDocWriterKey, Default)();
		}

		private static readonly object XmlDocWriterKey = new object();

		private static readonly Func<IXmlDocWriter> Default = () => new XmlDocWriter();
	}
}
