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
	public abstract class EmitContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// Construct a copy of the EmitContext.
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
		public static EmitContext<RootContext> Create()
		{
			return new EmitContext<RootContext>(
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
	

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.  For the most part it is
	/// readonly and copied whenever something needs to change.
	/// </summary>
	public class EmitContext<DocumentContextType> : EmitContext
		where DocumentContextType : DocumentContext
	{
		#region Constructors
		// =====================================================================

		/// <summary>
		/// This constructor is used to start the context chain.
		/// </summary>
		public EmitContext(DocumentContextType documentContext)
			: this(documentContext, new ConcurrentDictionary<object, object>(), ImmutableDictionary.Create<object, object>(), new EmitWriterContext())
		{
		}

		/// <summary>
		/// Construct a copy of the EmitContext.
		/// </summary>
		private EmitContext(DocumentContextType documentContext, ConcurrentDictionary<object, object> persistentDataMap, ImmutableDictionary<object, object> localDataMap, EmitWriterContext writerContext)
			:base(persistentDataMap, localDataMap, writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(documentContext != null);
			Contract.Ensures(this.m_documentContext != null);

			m_documentContext = documentContext;
		}

		// =====================================================================
		#endregion


		#region EmitContext Replace Functions
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


		#region Accessors
		// =====================================================================
		
		/// <summary>
		/// This property allows us to enter a select a new context from the existing context.
		/// </summary>
		public Detail.ContextSelector<DocumentContextType> Select
		{
			get
			{
				return new Detail.ContextSelector<DocumentContextType>(this);
			}
		}

		// =====================================================================
		#endregion


		#region Accessors
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


	#region Extension Support Methods
	// =====================================================================
	namespace EmitContextExtensionSupport
	{
		/// <summary>
		/// These are support functions that are useful when writing other EmitContext extension methods.
		/// </summary>
		public static class EmitContextExtensionSupportExtensions
		{
			/// <summary>
			/// Get the persistent data map from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the persistent data map.</param>
			/// <returns>The persistent data map of this context.</returns>
			public static ConcurrentDictionary<object, object> GetPersistentDataMap(this EmitContext context)
			{
				return EmitContext.GetPersistentDataMap(context);
			}

			/// <summary>
			/// Get the local data map from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the local data map.</param>
			/// <returns>The local data map of this context.</returns>
			public static ImmutableDictionary<object, object> GetLocalDataMap(this EmitContext context)
			{
				return EmitContext.GetLocalDataMap(context);
			}

			/// <summary>
			/// Get the writer context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the writer context.</param>
			/// <returns>The writer context of this context.</returns>
			public static EmitWriterContext GetWriterContext(this EmitContext context)
			{
				return EmitContext.GetWriterContext(context);
			}		
			
			/// <summary>
			/// Get the document context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the document context.</param>
			/// <returns>The document context of this context.</returns>
			public static DocumentContextType GetDocumentContext<DocumentContextType>(this EmitContext<DocumentContextType> context)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.GetDocumentContext(context);
			}

			
			/// <summary>
			/// Clone the emit context with a copy of the persistent data.
			/// </summary>
			/// <returns>A new emit context with a separate set of persistent data.</returns>
			public static EmitContext<DocumentContextType> ClonePersistentData<DocumentContextType>(this EmitContext<DocumentContextType> context)
				where DocumentContextType : DocumentContext
			{
				Contract.Requires(context != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				// Use ToArray instead of the IEnumerable interface to make sure we get a consistent snapshot.
				var newPersistentDataMap = new ConcurrentDictionary<object, object>(context.GetPersistentDataMap().ToArray());
				return EmitContext<DocumentContextType>.CopyWith(context, persistentDataMap: newPersistentDataMap);
			}

			/// <summary>
			/// Replace the emit target for this emit context.
			/// </summary>
			/// <param name="targetContext">The new target context.</param>
			/// <returns>A new emit context with the target context changed.</returns>
			public static EmitContext<DocumentContextType> ReplaceTargetContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitTargetContext targetContext)
				where DocumentContextType : DocumentContext
			{
				Contract.Requires(context != null);
				Contract.Requires(targetContext != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				// Register the target context.
				Script.CurrentRunContext.RegisterEmitTarget(targetContext);

				// Update the writer context.
				return ReplaceWriterContext(context, targetContext.WriterContext);
			}

			/// <summary>
			/// Get the existing EmitTargetContext from the data map using the given key, or if it doesn't exist call
			/// createFactory to create one and add it to the data map.  Replace the emit target for this emit context with
			/// the result.
			/// </summary>
			/// <param name="key">The key for this target.</param>
			/// <param name="createFunction">The EmitTargetContext factory function.</param>
			/// <returns>A new emit context with the target context changed.</returns>
			public static EmitContext<DocumentContextType> ReplaceTargetContext<DocumentContextType>(this EmitContext<DocumentContextType> context, object key, Func<EmitTargetContext> createFactory)
				where DocumentContextType : DocumentContext
			{
				return ReplaceTargetContext(context, (EmitTargetContext)context.GetPersistentDataMap().GetOrAdd(key, k => createFactory()));
			}

			/// <summary>
			/// Replace the writer context for this emit context.
			/// </summary>
			/// <param name="writerContext">The new writer context.</param>
			/// <returns>A new emit context with the writer context set to the given writer context.</returns>
			public static EmitContext<DocumentContextType> ReplaceWriterContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitWriterContext writerContext)
				where DocumentContextType : DocumentContext
			{
				Contract.Requires(context != null);
				Contract.Requires(writerContext != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				return context.GetWriterContext() != writerContext
					? EmitContext<DocumentContextType>.CopyWith(context, writerContext: writerContext)
					: context;
			}

			/// <summary>
			/// Replace the local data map for this emit context.
			/// </summary>
			/// <param name="localDataMap">The new local data map.</param>
			/// <returns>A new emit context with an updated local data map.</returns>
			public static EmitContext<DocumentContextType> ReplaceLocalDataMap<DocumentContextType>(this EmitContext<DocumentContextType> context, ImmutableDictionary<object, object> localDataMap)
				where DocumentContextType : DocumentContext
			{
				Contract.Requires(context != null);
				Contract.Requires(localDataMap != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				return context.GetLocalDataMap() != localDataMap
					? EmitContext<DocumentContextType>.CopyWith(context, localDataMap: localDataMap)
					: context;
			}

			/// <summary>
			/// Replace the formatter for this emit context.
			/// </summary>
			/// <param name="formatter">The formatter to be used for this context.</param>
			/// <returns>A new emit context with an updated formatter.</returns>
			public static EmitContext<DocumentContextType> ReplaceFormatterContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitFormatterContext formatter)
				where DocumentContextType : DocumentContext
			{
				return context.GetWriterContext().FormatterContext != formatter
					? ReplaceWriterContext(context, context.GetWriterContext().ReplaceFormatterContext(formatter))
					: context;
			}

			/// <summary>
			/// Replace the document context for this emit context.
			/// </summary>
			/// <param name="documentContext">The new document context to be used for this context.</param>
			/// <returns>A new emit context with an updated document context.</returns>
			public static EmitContext<NewDocumentContextType> ReplaceDocumentContext<NewDocumentContextType>(this EmitContext context, NewDocumentContextType documentContext)
				where NewDocumentContextType : DocumentContext
			{
				Contract.Requires(context != null);
				Contract.Requires(documentContext != null);
				Contract.Ensures(Contract.Result<NewDocumentContextType>() != null);

				return EmitContext<NewDocumentContextType>.CopyWith(context, documentContext);
			}
		}
	}

	// =====================================================================
	#endregion
}
