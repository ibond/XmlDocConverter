using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent.Detail
{
	/// <summary>
	/// An interface to the context selector.  This allows us to be covariant on the document context type.
	/// </summary>
	/// <typeparam name="TDocContext">The type of the document context.  This may be an interface implemented by a document context as well.</typeparam>
	public interface IContextSelector<EmitDocumentContextType, TParentContext, out TDocContext>
		where EmitDocumentContextType : DocumentContext
		where TParentContext : EmitContext
	{
		/// <summary>
		/// Gets the EmitContext for this context selector.
		/// </summary>
		EmitContext<EmitDocumentContextType, TParentContext> EmitContext { get; }

		/// <summary>
		/// Gets the document context for this context selector.
		/// </summary>
		TDocContext DocumentContext { get; }
	}

	/// <summary>
	/// This is a utility class that allows us to use the .Select.Assemblies syntax.
	/// </summary>
	public class ContextSelector<TDocContext, TParentContext> : IContextSelector<TDocContext, TParentContext, TDocContext>
		where TDocContext : DocumentContext
		where TParentContext : EmitContext
	{
		/// <summary>
		/// Construct a context selector.
		/// </summary>
		/// <param name="emitContext">The emit context.</param>
		public ContextSelector(EmitContext<TDocContext, TParentContext> emitContext)
		{
			Contract.Requires(emitContext != null);
			Contract.Ensures(m_emitContext != null);

			m_emitContext = emitContext;
		}

		/// <summary>
		/// Get the emit context.
		/// </summary>
		public EmitContext<TDocContext, TParentContext> EmitContext { get { return m_emitContext; } }

		/// <summary>
		/// Get the document context.
		/// </summary>
		public TDocContext DocumentContext { get { return m_emitContext.GetDocumentContext(); } }

		/// <summary>
		/// The emit context.
		/// </summary>
		private readonly EmitContext<TDocContext, TParentContext> m_emitContext;
	}
}
