﻿using System;
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
	/// This is a context representing a specific element (e.g. summary, remarks, etc) within a document entry.
	/// </summary>
	public class DocElementContext : DotNetDocumentContext<DocElementContext>
	{
		/// <summary>
		/// Construct an DocElementContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public DocElementContext(DocumentSource documentSource, XElement element)
			: base(documentSource)
		{
			Contract.Requires(element != null);

			m_element = element;
		}

		protected override EmitWriter<DocElementContext>.Writer GetDefaultRenderer()
		{
			return item =>
				{
					item.Emit.WriteXmlElement();
				};
		}
		
		/// <summary>
		/// Access the element directly.
		/// </summary>
		public XElement Element { get { return m_element; } }

		/// <summary>
		/// The document element.
		/// </summary>
		private readonly XElement m_element;

		/// <summary>
		/// The interface for getting a documentation element.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get the documentation element.
			/// </summary>
			DocElementContext GetDocElement(string elementName);
		}
	}

	/// <summary>
	/// Selector extensions for DocElementContext.
	/// </summary>
	public static class DocElementContextProviderExtensions
	{
		public static EmitContext<TDoc>

			Element<TDoc>(
				this IContextSelector<TDoc, DocElementContext.IProvider> selector,
				Action<EmitContext<DocElementContext>> action, 
				string elementName)

			where TDoc : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocElementContext>>() != null);

			action(selector.EmitContext
				.ReplaceDocumentContext(selector.DocumentContext.GetDocElement(elementName)));

			return selector.EmitContext;
		}
	}
}
