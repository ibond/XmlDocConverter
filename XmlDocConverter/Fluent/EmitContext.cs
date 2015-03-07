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

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This contains functionality not tied to a specific document context.
	/// </summary>
	public static class EmitContext
	{
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
	}

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.  For the most part it is
	/// readonly and copied whenever something needs to change.
	/// </summary>
	public class EmitContext<DocumentContextType>
		where DocumentContextType : DocumentContext
	{
		#region EmitContext Constructors
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
		{
			// We must always have something for these values.
			Contract.Requires(documentContext != null);
			Contract.Requires(persistentDataMap != null);
			Contract.Requires(localDataMap != null);
			Contract.Requires(writerContext != null);

			Contract.Ensures(this.m_documentContext != null);
			Contract.Ensures(this.m_persistentDataMap != null);
			Contract.Ensures(this.m_localDataMap != null);
			Contract.Ensures(this.m_writerContext != null);

			m_persistentDataMap = persistentDataMap;
			m_localDataMap = localDataMap;
			m_documentContext = documentContext;
			m_writerContext = writerContext;
		}

		// =====================================================================
		#endregion


		#region EmitContext Replace Functions
		// =====================================================================
		//
		// These functions are intended to be used by the extension methods and not as part of the fluent interface.  To
		// prevent them from showing up when using intellisense we make them static.

		/// <summary>
		/// Clone the emit context with a copy of the persistent data.
		/// </summary>
		/// <returns>A new emit context with a separate set of persistent data.</returns>
		public static EmitContext<DocumentContextType> ClonePersistentData(EmitContext<DocumentContextType> context)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<DocumentContextType>() != null);

			// Use ToArray instead of the IEnumerable interface to make sure we get a consistent snapshot.
			var newPersistentDataMap = new ConcurrentDictionary<object, object>(context.m_persistentDataMap.ToArray());
			return new EmitContext<DocumentContextType>(context.m_documentContext, newPersistentDataMap, context.m_localDataMap, context.m_writerContext);
		}

		/// <summary>
		/// Replace the emit target for this emit context.
		/// </summary>
		/// <param name="targetContext">The new target context.</param>
		/// <returns>A new emit context with the target context changed.</returns>
		public static EmitContext<DocumentContextType> ReplaceTargetContext(EmitContext<DocumentContextType> context, EmitTargetContext targetContext)
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
		public static EmitContext<DocumentContextType> ReplaceTargetContext(EmitContext<DocumentContextType> context, object key, Func<EmitTargetContext> createFactory)
		{
			return ReplaceTargetContext(context, (EmitTargetContext)context.m_persistentDataMap.GetOrAdd(key, k => createFactory()));
		}

		/// <summary>
		/// Replace the writer context for this emit context.
		/// </summary>
		/// <param name="writerContext">The new writer context.</param>
		/// <returns>A new emit context with the writer context set to the given writer context.</returns>
		public static EmitContext<DocumentContextType> ReplaceWriterContext(EmitContext<DocumentContextType> context, EmitWriterContext writerContext)
		{
			Contract.Requires(context != null);
			Contract.Requires(writerContext != null);
			Contract.Ensures(Contract.Result<DocumentContextType>() != null);

			return context.m_writerContext != writerContext
				? new EmitContext<DocumentContextType>(context.m_documentContext, context.m_persistentDataMap, context.m_localDataMap, writerContext)
				: context;
		}

		/// <summary>
		/// Replace the local data map for this emit context.
		/// </summary>
		/// <param name="localDataMap">The new local data map.</param>
		/// <returns>A new emit context with an updated local data map.</returns>
		public static EmitContext<DocumentContextType> ReplaceLocalDataMap(EmitContext<DocumentContextType> context, ImmutableDictionary<object, object> localDataMap)
		{
			Contract.Requires(context != null);
			Contract.Requires(localDataMap != null);
			Contract.Ensures(Contract.Result<DocumentContextType>() != null);

			return context.m_localDataMap != localDataMap
				? new EmitContext<DocumentContextType>(context.m_documentContext, context.m_persistentDataMap, localDataMap, context.m_writerContext)
				: context;
		}

		/// <summary>
		/// Replace the formatter for this emit context.
		/// </summary>
		/// <param name="formatter">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<DocumentContextType> ReplaceFormatterContext(EmitContext<DocumentContextType> context, EmitFormatterContext formatter)
		{
			return ReplaceWriterContext(context, context.m_writerContext.ReplaceFormatterContext(formatter));
		}

		/// <summary>
		/// Replace the document context for this emit context.
		/// </summary>
		/// <param name="documentContext">The new document context to be used for this context.</param>
		/// <returns>A new emit context with an updated document context.</returns>
		public static EmitContext<NewDocumentContextType> ReplaceDocumentContext<NewDocumentContextType>(EmitContext<DocumentContextType> context, NewDocumentContextType documentContext)
			where NewDocumentContextType : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<DocumentContextType>() != null);

			return new EmitContext<NewDocumentContextType>(documentContext, context.m_persistentDataMap, context.m_localDataMap, context.m_writerContext);
		}

		// =====================================================================
		#endregion


		#region Accessors
		// =====================================================================

		public Detail.IContextSelector<DocumentContextType, DocumentContextType> Select
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
		/// Gets the emit writer context.
		/// </summary>
		public EmitWriterContext WriterContext { get { return m_writerContext; } }
		
		/// <summary>
		/// Gets the document context.
		/// </summary>
		public DocumentContextType DocumentContext { get { return m_documentContext; } }

		// =====================================================================
		#endregion


		#region Private Members
		// =====================================================================

		/// <summary>
		/// Get the persistent data map for this context.
		/// </summary>
		public ConcurrentDictionary<object, object> PersistentDataMap { get { return m_persistentDataMap; } }

		/// <summary>
		/// Get the local data map for this context.
		/// </summary>
		public ImmutableDictionary<object, object> LocalDataMap { get { return m_localDataMap; } }
		
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
		/// The current context within the document.
		/// </summary>
		private readonly DocumentContextType m_documentContext;

		/// <summary>
		/// The emit writer context.
		/// </summary>
		private readonly EmitWriterContext m_writerContext;

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
			/// Clone the emit context with a copy of the persistent data.
			/// </summary>
			/// <returns>A new emit context with a separate set of persistent data.</returns>
			public static EmitContext<DocumentContextType> ClonePersistentData<DocumentContextType>(this EmitContext<DocumentContextType> context)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ClonePersistentData(context);
			}

			/// <summary>
			/// Replace the emit target for this emit context.
			/// </summary>
			/// <param name="targetContext">The new target context.</param>
			/// <returns>A new emit context with the target context changed.</returns>
			public static EmitContext<DocumentContextType> ReplaceTargetContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitTargetContext targetContext)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ReplaceTargetContext(context, targetContext);
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
				return EmitContext<DocumentContextType>.ReplaceTargetContext(context, key, createFactory);
			}

			/// <summary>
			/// Replace the writer context for this emit context.
			/// </summary>
			/// <param name="writerContext">The new writer context.</param>
			/// <returns>A new emit context with the writer context set to the given writer context.</returns>
			public static EmitContext<DocumentContextType> ReplaceWriterContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitWriterContext writerContext)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ReplaceWriterContext(context, writerContext);
			}

			/// <summary>
			/// Replace the local data map for this emit context.
			/// </summary>
			/// <param name="localDataMap">The new local data map.</param>
			/// <returns>A new emit context with an updated local data map.</returns>
			public static EmitContext<DocumentContextType> ReplaceLocalDataMap<DocumentContextType>(this EmitContext<DocumentContextType> context, ImmutableDictionary<object, object> localDataMap)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ReplaceLocalDataMap(context, localDataMap);
			}

			/// <summary>
			/// Replace the formatter for this emit context.
			/// </summary>
			/// <param name="formatter">The formatter to be used for this context.</param>
			/// <returns>A new emit context with an updated formatter.</returns>
			public static EmitContext<DocumentContextType> ReplaceFormatterContext<DocumentContextType>(this EmitContext<DocumentContextType> context, EmitFormatterContext formatter)
				where DocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ReplaceFormatterContext(context, formatter);
			}

			/// <summary>
			/// Replace the document context for this emit context.
			/// </summary>
			/// <param name="documentContext">The new document context to be used for this context.</param>
			/// <returns>A new emit context with an updated document context.</returns>
			public static EmitContext<NewDocumentContextType> ReplaceDocumentContext<DocumentContextType, NewDocumentContextType>(this EmitContext<DocumentContextType> context, NewDocumentContextType documentContext)
				where DocumentContextType : DocumentContext
				where NewDocumentContextType : DocumentContext
			{
				return EmitContext<DocumentContextType>.ReplaceDocumentContext(context, documentContext);
			}
		}
	}

	// =====================================================================
	#endregion
}
