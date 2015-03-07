using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	public abstract class DocumentContext
	{
	}

	public abstract class ScalarDocumentContext : DocumentContext
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

	public class DocumentContextCollection<DocumentContextType> : DocumentContext, IDocumentContextCollection<DocumentContextType>
		where DocumentContextType : DocumentContext
	{
		public DocumentContextCollection(IEnumerable<DocumentContextType> elements)
		{
			m_elements = elements;
		}

		public IEnumerable<DocumentContextType> Elements { get { return m_elements; } }

		private readonly IEnumerable<DocumentContextType> m_elements;
	}
}
