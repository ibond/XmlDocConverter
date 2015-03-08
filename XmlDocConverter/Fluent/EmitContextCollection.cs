//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Contracts;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace XmlDocConverter.Fluent
//{
//	/// <summary>
//	/// This represents a collection of emit contexts.
//	/// </summary>
//	/// <typeparam name="TDocContext">The type of the document contexts contained in this collection.</typeparam>
//	public class EmitContextCollection<TDocContext, SourceDocumentContextType>
//		where TDocContext : DocumentContext
//		where SourceDocumentContextType : DocumentContext
//	{
//		/// <summary>
//		/// Construct an EmitContextCollection.
//		/// </summary>
//		/// <param name="contexts">The contexts contained within this collection.</param>
//		public EmitContextCollection(IEnumerable<EmitContext<TDocContext>> contexts, EmitContext<SourceDocumentContextType> sourceContext)
//		{
//			Contract.Requires(contexts != null);
//			Contract.Requires(sourceContext != null);
//			Contract.Ensures(m_contexts != null);
//			Contract.Ensures(m_sourceContext != null);

//			m_contexts = contexts;
//			m_sourceContext = sourceContext;
//		}

//		/// <summary>
//		/// Gets the emit contexts.
//		/// </summary>
//		public IEnumerable<EmitContext<TDocContext>> Contexts { get { return m_contexts; } }

//		/// <summary>
//		/// Gets the emit contexts.
//		/// </summary>
//		public EmitContext<SourceDocumentContextType> Source { get { return m_sourceContext; } }

//		/// <summary>
//		/// The source context that generated this context collection.
//		/// </summary>
//		private readonly EmitContext<SourceDocumentContextType> m_sourceContext;

//		/// <summary>
//		/// The contexts contained within this group.
//		/// </summary>
//		private readonly IEnumerable<EmitContext<TDocContext>> m_contexts;
//	}
//}
