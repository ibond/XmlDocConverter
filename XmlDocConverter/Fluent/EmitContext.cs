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
		public static EmitContextRoot<RootContext> CreateRoot()
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
		public static EmitContextRoot<TDoc> CreateRoot<TDoc, TParent>(EmitContext<TDoc, TParent> emitContext)
			where TDoc : DocumentContext
			where TParent : EmitContext
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
		public static EmitContextRoot<TDoc> CreateRoot<TDoc>(
			TDoc documentContext, 
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			where TDoc : DocumentContext
		{
			return new EmitContextRoot<TDoc>(documentContext, persistentDataMap, localDataMap, writerContext);
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
		/// 
		/// Specifically this makes it unnecessary to wrap emit chains in @{...} in Razor templates.
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
	public class EmitContext<TDoc, TParent> : EmitContext
		where TDoc : DocumentContext
		where TParent : EmitContext
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
		protected EmitContext(
			TDoc documentContext,
			Func<EmitContext<TDoc, TParent>, TParent> getParentContext,
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			: base(persistentDataMap, localDataMap, writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(documentContext != null);
			Contract.Requires(getParentContext != null);
			Contract.Ensures(m_documentContext != null);
			Contract.Ensures(m_parentContext != null);

			m_documentContext = documentContext;
			m_parentContext = getParentContext(this);
		}

		/// <summary>
		/// Convert this to a more general EmitContext&lt;TDoc&gt; where the parent context is upcasted to
		/// EmitContext.
		/// </summary>
		/// <param name="context">The context to convert.</param>
		/// <returns>A copy of the emit context with the type modified.</returns>
		public static implicit operator EmitContext<TDoc>(EmitContext<TDoc, TParent> context)
		{
			return new EmitContext<TDoc>(
				context.m_documentContext,
				context.m_parentContext,
				context.m_persistentDataMap,
				context.m_localDataMap,
				context.m_writerContext);
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
		public static EmitContext<TDoc, TParent> CopyWith(
			EmitContext<TDoc, TParent> sourceContext,
			TParent parentContext = null,
			TDoc documentContext = null,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
		{
			Contract.Requires(sourceContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDoc, TParent>>() != null);

			// Create the new context.
			return new EmitContext<TDoc, TParent>(
				documentContext ?? sourceContext.m_documentContext,
				self => parentContext ?? sourceContext.m_parentContext,
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
		public static EmitContext<NewDocumentContextType, TParent> CopyWith<NewDocumentContextType>(
			EmitContext sourceContext,
			NewDocumentContextType documentContext,
			TParent parentContext,
			ConcurrentDictionary<object, object> persistentDataMap = null,
			ImmutableDictionary<object, object> localDataMap = null,
			EmitWriterContext writerContext = null)
			where NewDocumentContextType : DocumentContext
		{
			Contract.Requires(sourceContext != null);
			Contract.Requires(documentContext != null);
			Contract.Requires(parentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType, TParent>>() != null);

			// Create the new context.
			return new EmitContext<NewDocumentContextType, TParent>(
				documentContext,
				self => parentContext,
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
		public Detail.ContextSelector<TDoc, TParent> Select
		{
			get
			{
				return new Detail.ContextSelector<TDoc, TParent>(this);
			}
		}

		public WriteSelector<TDoc, TParent> Write
		{
			get
			{
				return new WriteSelector<TDoc, TParent>(this);
			}
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
		public static TDoc GetDocumentContext(EmitContext<TDoc, TParent> context)
		{
			return context.m_documentContext;
		}

		/// <summary>
		/// Get the parent context from the context.
		/// 
		/// This function is meant for internal use and extension implementors so it is made static to keep the fluent
		/// interface clean.
		/// </summary>
		/// <param name="context">The context from which we should get the parent context.</param>
		/// <returns>The parent of this context.</returns>
		public static TParent GetParentContext(EmitContext<TDoc, TParent> context)
		{
			return context.m_parentContext;
		}

		// =====================================================================
		#endregion


		#region Private Members
		// =====================================================================

		/// <summary>
		/// The current context within the document.
		/// </summary>
		private readonly TDoc m_documentContext;

		/// <summary>
		/// The parent emit context.
		/// 
		/// Use the ParentContext property to access the parent context as this may be overridden in a derived class.
		/// </summary>
		private readonly TParent m_parentContext;

		// =====================================================================
		#endregion
	}


	/// <summary>
	/// This is a function to make it easier to deal with types where the parent context type doesn't matter.
	/// </summary>
	/// <typeparam name="TDoc">The type of the document context.</typeparam>
	public class EmitContext<TDoc> : EmitContext<TDoc, EmitContext>
		where TDoc : DocumentContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// Construct a copy of the EmitContext.
		/// </summary>
		internal EmitContext(
			TDoc documentContext,
			EmitContext parentContext,
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			: base(documentContext, self => parentContext, persistentDataMap, localDataMap, writerContext)
		{
		}

		// =====================================================================
		#endregion
	}

	
	/// <summary>
	/// This is a special type of the EmitContext for representing the top level of an emit context chain.  It's
	/// effectively a normal emit context except the parent points back to itself.
	/// </summary>
	/// <typeparam name="TDoc">The document context type.</typeparam>
	public class EmitContextRoot<TDoc> : EmitContext<TDoc, EmitContextRoot<TDoc>>
		where TDoc : DocumentContext
	{
		internal EmitContextRoot(
			TDoc documentContext,
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitWriterContext writerContext)
			: base(
				documentContext,
				self => (EmitContextRoot<TDoc>)self,
				persistentDataMap,
				localDataMap,
				writerContext)
		{
		}
	}
}
