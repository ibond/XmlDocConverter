using NuDoq;
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
	/// This contains functionality not tied to a specific document context.
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
		/// Create a new empty emit context.
		/// </summary>
		/// <returns>A new emit context with default values.</returns>
		public static EmitContext<RootContext, EmitContext> Create()
		{
			return new EmitContext<RootContext, EmitContext>(
					new EmitContext(new ConcurrentDictionary<object, object>(), ImmutableDictionary.Create<object, object>(), new EmitWriterContext()),
					new RootContext(
						new DocumentSource(ImmutableList.Create<AssemblyMembers>())));
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
		
		// =====================================================================
		#endregion


		#region Protected Members
		// =====================================================================

		/// <summary>
		/// A persistent general use data map.  This map will be shared with all emit contexts that are copied from the
		/// current context and updates will be seen in all contexts.
		/// </summary>
		private readonly ConcurrentDictionary<object, object> m_persistentDataMap;

		/// <summary>
		/// A local general use data map.  Every copy of the emit context will see it's own version of this map.
		/// </summary>
		private readonly ImmutableDictionary<object, object> m_localDataMap;

		/// <summary>
		/// The emit writer context.
		/// </summary>
		private readonly EmitWriterContext m_writerContext;

		// =====================================================================
		#endregion
	}

	public class EmitContext<DocumentContextType> : EmitContext
		where DocumentContextType : DocumentContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// Construct a copy of the EmitContext.
		/// </summary>
		protected EmitContext(
			DocumentContextType documentContext, 
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap, 
			EmitWriterContext writerContext)
			:base(persistentDataMap, localDataMap, writerContext)
		{
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
		public static EmitContext<DocumentContextType> CopyWith(
			EmitContext<DocumentContextType> sourceContext,
			DocumentContextType documentContext = null,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
		{
			Contract.Requires(sourceContext != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextType>>() != null);

			// Create the new context.
			return new EmitContext<DocumentContextType>(
				documentContext ?? sourceContext.GetDocumentContext(),
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
		public static DocumentContextType GetDocumentContext(EmitContext<DocumentContextType> context)
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
		private readonly DocumentContextType m_documentContext;

		// =====================================================================
		#endregion
	}
	

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.  For the most part it is
	/// readonly and copied whenever something needs to change.
	/// </summary>
	public class EmitContext<DocumentContextType, ParentEmitContextType> : EmitContext<DocumentContextType>
		where DocumentContextType : DocumentContext
		where ParentEmitContextType : EmitContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// This constructor is used to start the context chain.
		/// </summary>
		public EmitContext(ParentEmitContextType parentContext, DocumentContextType documentContext)
			: this(parentContext, documentContext, new ConcurrentDictionary<object, object>(), ImmutableDictionary.Create<object, object>(), new EmitWriterContext())
		{
		}

		/// <summary>
		/// Construct a copy of the EmitContext.
		/// </summary>
		private EmitContext(
			ParentEmitContextType parentContext, 
			DocumentContextType documentContext, 
			ConcurrentDictionary<object, object> persistentDataMap, 
			ImmutableDictionary<object, object> localDataMap, 
			EmitWriterContext writerContext)
			: base(documentContext, persistentDataMap, localDataMap, writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(parentContext != null);
			Contract.Ensures(this.m_parentContext != null);

			m_parentContext = parentContext;
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
		public static EmitContext<DocumentContextType, ParentEmitContextType> CopyWith(
			EmitContext<DocumentContextType, ParentEmitContextType> sourceContext,
			ParentEmitContextType parentContext = null,
			DocumentContextType documentContext = null,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
		{
			Contract.Requires(sourceContext != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextType, ParentEmitContextType>>() != null);

			// Create the new context.
			return new EmitContext<DocumentContextType, ParentEmitContextType>(
				parentContext ?? sourceContext.GetParentContext(),
				documentContext ?? sourceContext.GetDocumentContext(),
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
		public static EmitContext<NewDocumentContextType, ParentEmitContextType> CopyWith<NewDocumentContextType>(
			EmitContext sourceContext,
			ParentEmitContextType parentContext,
			NewDocumentContextType documentContext,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
			where NewDocumentContextType : DocumentContext
		{
			Contract.Requires(sourceContext != null);
			Contract.Requires(documentContext != null);
			Contract.Requires(parentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType, ParentEmitContextType>>() != null);

			// Create the new context.
			return new EmitContext<NewDocumentContextType, ParentEmitContextType>(
				parentContext,
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
		/// This property allows us to enter a select a new context from the existing context.
		/// </summary>
		public Detail.ContextSelector<DocumentContextType, ParentEmitContextType> Select
		{
			get
			{
				return new Detail.ContextSelector<DocumentContextType, ParentEmitContextType>(this);
			}
		}

		// =====================================================================
		#endregion


		#region Static Accessors
		// =====================================================================

		/// <summary>
		/// Get the parent context from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the parent context.</param>
		/// <returns>The parent of this context.</returns>
		public static ParentEmitContextType GetParentContext(EmitContext<DocumentContextType, ParentEmitContextType> context)
		{
			return context.m_parentContext;
		}

		// =====================================================================
		#endregion


		#region Private Members
		// =====================================================================
		
		/// <summary>
		/// The parent emit context.
		/// </summary>
		private readonly ParentEmitContextType m_parentContext;

		// =====================================================================
		#endregion
	}
}
