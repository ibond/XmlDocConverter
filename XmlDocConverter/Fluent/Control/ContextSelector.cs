using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent.Detail
{
	public interface IContextSelector<EmitDocumentContextType, out DocumentContextType>
		where EmitDocumentContextType : DocumentContext
	{
		EmitContext<EmitDocumentContextType> EmitContext { get; }
		DocumentContextType DocumentContext { get; }
	}

	/// <summary>
	/// This is a utility class that allows us to use the .Select.Assemblies syntax.
	/// </summary>
	public class ContextSelector<DocumentContextType> : IContextSelector<DocumentContextType, DocumentContextType>
		where DocumentContextType : DocumentContext
	{
		/// <summary>
		/// Construct a context selector.
		/// </summary>
		/// <param name="emitContext">The emit context.</param>
		public ContextSelector(EmitContext<DocumentContextType> emitContext)
		{
			Contract.Requires(emitContext != null);
			Contract.Ensures(m_emitContext != null);

			m_emitContext = emitContext;
		}

		/// <summary>
		/// Get the emit context.
		/// </summary>
		public EmitContext<DocumentContextType> EmitContext { get { return m_emitContext; } }

		/// <summary>
		/// Get the document context.
		/// </summary>
		public DocumentContextType DocumentContext { get { return m_emitContext.DocumentContext; } }

		/// <summary>
		/// The emit context.
		/// </summary>
		private readonly EmitContext<DocumentContextType> m_emitContext;
	}
}
