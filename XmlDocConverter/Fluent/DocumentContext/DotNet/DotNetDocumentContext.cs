using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is a base document context for any .NET context types.
	/// </summary>
	public abstract class DotNetDocumentContext<TDerived> : DocumentContext<TDerived>
		where TDerived : DotNetDocumentContext<TDerived>
	{
		/// <summary>
		/// Construct a DocumentContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public DotNetDocumentContext(DocumentSource documentSource)
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
}
