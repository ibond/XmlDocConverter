using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This contains functionality not tied to a specific document context or parent context.
	/// </summary>
	public class EmitContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// Construct an EmitContext.
		/// </summary>
		protected EmitContext(ConcurrentDictionary<object, object> persistentDataMap, ImmutableDictionary<object, object> localDataMap, EmitWriterContext writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(persistentDataMap != null);
			Contract.Requires(localDataMap != null);
			Contract.Requires(writerContext != null);

			Contract.Ensures(this.m_persistentDataMap != null);
			Contract.Ensures(this.m_localDataMap != null);
			Contract.Ensures(this.m_writerContext != null);

			m_persistentDataMap = persistentDataMap;
			m_localDataMap = localDataMap;
			m_writerContext = writerContext;
		}

		// =====================================================================
		#endregion


		#region Static Members
		// =====================================================================

		/// <summary>
		/// Create a new root emit context.
		/// </summary>
		/// <returns>A new emit context with default values.</returns>
		public static EmitContext<RootContext> CreateRoot()
		{
			return CreateRoot(
				new RootContext(new DocumentSource(Enumerable.Empty<XmlDocPathPair>())),
				new ConcurrentDictionary<object,object>(),
				ImmutableDictionary<object,object>.Empty,
				new EmitWriterContext());
		}

		/// <summary>
		/// Create a new root emit context based on an existing context.
		/// </summary>
		/// <returns>A new emit context with default values.</returns>
		public static EmitContext<TDoc> CreateRoot<TDoc>(EmitContext<TDoc> emitContext)
			where TDoc : DocumentContext
		{
			return CreateRoot(
				emitContext.GetDocumentContext(),
				emitContext.GetPersistentDataMap(),
				emitContext.GetLocalDataMap(),
				emitContext.GetWriterContext());
		}

		/// <summary>
		/// Create a new root emit context.
		/// </summary>
		/// <returns>A new emit context with the specified values.</returns>
		public static EmitContext<TDoc> CreateRoot<TDoc>(
			TDoc documentContext, 
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			where TDoc : DocumentContext
		{
			return new EmitContext<TDoc>(documentContext, persistentDataMap, localDataMap, writerContext);
		}
		
		// =====================================================================
		#endregion


		#region Accessors
		// =====================================================================

		/// <summary>
		/// Get the persistent data map from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the persistent data map.</param>
		/// <returns>The persistent data map of this context.</returns>
		public static ConcurrentDictionary<object, object> GetPersistentDataMap(EmitContext context)
		{
			return context.m_persistentDataMap;
		}

		/// <summary>
		/// Get the local data map from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the local data map.</param>
		/// <returns>The local data map of this context.</returns>
		public static ImmutableDictionary<object, object> GetLocalDataMap(EmitContext context)
		{
			return context.m_localDataMap;
		}

		/// <summary>
		/// Get the writer context from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the writer context.</param>
		/// <returns>The writer context of this context.</returns>
		public static EmitWriterContext GetWriterContext(EmitContext context)
		{
			return context.m_writerContext;
		}

		/// <summary>
		/// Don't return anything from ToString() to prevent this object from putting unintended text into templates.
		/// </summary>
		/// <returns>An empty string.</returns>
		public override string ToString()
		{
			return String.Empty;
		}
		
		// =====================================================================
		#endregion


		#region Protected Members
		// =====================================================================

		/// <summary>
		/// A persistent general use data map.  This map will be shared with all emit contexts that are copied from the
		/// current context and updates will be seen in all contexts.
		/// </summary>
		protected readonly ConcurrentDictionary<object, object> m_persistentDataMap;

		/// <summary>
		/// A local general use data map.  Every copy of the emit context will see it's own version of this map.
		/// </summary>
		protected readonly ImmutableDictionary<object, object> m_localDataMap;

		/// <summary>
		/// The emit writer context.
		/// </summary>
		protected readonly EmitWriterContext m_writerContext;

		// =====================================================================
		#endregion
	}
		

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.  For the most part it is
	/// readonly and copied whenever something needs to change.
	/// </summary>
	public class EmitContext<TDoc> : EmitContext
		where TDoc : DocumentContext
	{
		#region Constructors and Conversions
		// =====================================================================

		/// <summary>
		/// Construct a new emit context.
		/// </summary>
		/// <param name="documentContext">The document context.</param>
		/// <param name="getParentContext">
		/// A function to get the parent context of the emit context.  This is a function to allow self-referencing
		/// contexts, it will be called in this constructor to set the parent context.
		/// </param>
		/// <param name="persistentDataMap">The persistent data map.</param>
		/// <param name="localDataMap">The local data map.</param>
		/// <param name="writerContext">The writer context.</param>
		internal EmitContext(
			TDoc documentContext,
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			: base(persistentDataMap, localDataMap, writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(documentContext != null);
			Contract.Ensures(m_documentContext != null);

			m_documentContext = documentContext;
		}

		// =====================================================================
		#endregion


		#region EmitContext Copy Functions
		// =====================================================================
		
		/// <summary>
		/// Copy the given emit context with replaced values.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="sourceContext">The source context.</param>
		/// <param name="documentContext">The new value for the document context.  If null this will copy from the source context instead.</param>
		/// <param name="persistentDataMap">The new value for the persistent data map.  If null this will copy from the source context instead.</param>
		/// <param name="localDataMap">The new value for the local data map.  If null this will copy from the source context instead.</param>
		/// <param name="writerContext">The new value for the writer context.  If null this will copy from the source context instead.</param>
		/// <returns>A new emit context with replaced values.</returns>
		public static EmitContext<TDoc> CopyWith(
			EmitContext<TDoc> sourceContext,
			TDoc documentContext = null,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
		{
			Contract.Requires(sourceContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDoc>>() != null);

			// Create the new context.
			return new EmitContext<TDoc>(
				documentContext ?? sourceContext.m_documentContext,
				persistentDataMap ?? sourceContext.GetPersistentDataMap(),
				localDataMap ?? sourceContext.GetLocalDataMap(),
				writerContext ?? sourceContext.GetWriterContext());
		}

		/// <summary>
		/// Copy the given emit context with replaced values.  This overload allows copying from any EmitContext type.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="sourceContext">The source context.</param>
		/// <param name="documentContext">The new value for the document context.</param>
		/// <param name="persistentDataMap">The new value for the persistent data map.  If null this will copy from the source context instead.</param>
		/// <param name="localDataMap">The new value for the local data map.  If null this will copy from the source context instead.</param>
		/// <param name="writerContext">The new value for the writer context.  If null this will copy from the source context instead.</param>
		/// <returns>A new emit context with replaced values.</returns>
		public static EmitContext<NewDocumentContextType> CopyWith<NewDocumentContextType>(
			EmitContext sourceContext,
			NewDocumentContextType documentContext,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
			where NewDocumentContextType : DocumentContext
		{
			Contract.Requires(sourceContext != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType>>() != null);

			// Create the new context.
			return new EmitContext<NewDocumentContextType>(
				documentContext,
				persistentDataMap ?? sourceContext.GetPersistentDataMap(),
				localDataMap ?? sourceContext.GetLocalDataMap(),
				writerContext ?? sourceContext.GetWriterContext());
		}

		// =====================================================================
		#endregion


		#region Accessors
		// =====================================================================
		
		/// <summary>
		/// This property allows us to enter a select context from the existing context.
		/// </summary>
		public Detail.ContextSelector<TDoc> Select
		{
			get
			{
				return new Detail.ContextSelector<TDoc>(this);
			}
		}

		public WriteSelector<TDoc> Write
		{
			get
			{
				return new WriteSelector<TDoc>(this);
			}
		}

		public TDoc Item { get { return m_documentContext; } }

		// =====================================================================
		#endregion


		#region Static Accessors
		// =====================================================================
		
		/// <summary>
		/// Get the document context from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the document context.</param>
		/// <returns>The document context of this context.</returns>
		public static TDoc GetDocumentContext(EmitContext<TDoc> context)
		{
			return context.m_documentContext;
		}
		
		// =====================================================================
		#endregion


		#region Private Members
		// =====================================================================

		/// <summary>
		/// The current context within the document.
		/// </summary>
		private readonly TDoc m_documentContext;

		// =====================================================================
		#endregion
	}	
}
