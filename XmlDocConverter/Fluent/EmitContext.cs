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
	

	/// <summary>
	/// This type contains the necessary context for deciding what, where, and how to emit.  For the most part it is
	/// readonly and copied whenever something needs to change.
	/// </summary>
	public class EmitContext<DocumentContextType, ParentEmitContextType> : EmitContext
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
			:base(persistentDataMap, localDataMap, writerContext)
		{
			// We must always have something for these values.
			Contract.Requires(parentContext != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(this.m_parentContext != null);
			Contract.Ensures(this.m_documentContext != null);

			m_parentContext = parentContext;
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
		public static DocumentContextType GetDocumentContext(EmitContext<DocumentContextType, ParentEmitContextType> context)
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
		public static ParentEmitContextType GetParentContext(EmitContext<DocumentContextType, ParentEmitContextType> context)
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
		private readonly DocumentContextType m_documentContext;

		/// <summary>
		/// The parent emit context.
		/// </summary>
		private readonly ParentEmitContextType m_parentContext;

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
			/// Replace the local data map for this emit context.
			/// </summary>
			/// <param name="localDataMap">The new local data map.</param>
			/// <returns>A new emit context with an updated local data map.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> ReplaceLocalDataMap<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, ImmutableDictionary<object, object> localDataMap)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Requires(localDataMap != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				return context.GetLocalDataMap() != localDataMap
					? EmitContext<DocumentContextType, ParentEmitContextType>.CopyWith(context, localDataMap: localDataMap)
					: context;
			}

			/// <summary>
			/// Get an object from the local data map.  If the object doesn't exist in the map return the given default value.
			/// </summary>
			/// <param name="key">The local data map key.</param>
			/// <param name="context">The context from which we should get the data.</param>
			/// <param name="defaultValue">The default value to be returned if the key does not exist in the data map.</param>
			/// <returns>If the key exists in the local data map this returns the corresponding value, otherwise returns defaultValue.</returns>
			public static DataType GetLocalData<DataType>(this EmitContext context, object key, DataType defaultValue)
			{
				Contract.Requires(context != null);
				Contract.Requires(key != null);

				object value;
				return context.GetLocalDataMap().TryGetValue(key, out value)
					? (DataType)value
					: defaultValue;
			}

			/// <summary>
			/// Set an item in the local data map.
			/// </summary>
			/// <param name="context">The context to be modified.</param>
			/// <param name="key">The key into the local data map.</param>
			/// <param name="value">The value to be added to the local data map.</param>
			/// <returns>A new emit context with an updated local data map.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> 
				SetLocalData<DocumentContextType, ParentEmitContextType, DataType>(
					this EmitContext<DocumentContextType, ParentEmitContextType> context,
					object key, 
					DataType value)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Requires(key != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				return context.ReplaceLocalDataMap(context.GetLocalDataMap().SetItem(key, value));
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
			/// Get the formatter context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the formatter context.</param>
			/// <returns>The formatter context of this context.</returns>
			public static EmitFormatterContext GetFormatterContext(this EmitContext context)
			{
				return EmitContext.GetWriterContext(context).FormatterContext;
			}

			/// <summary>
			/// Get the output context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the output context.</param>
			/// <returns>The output context of this context.</returns>
			public static EmitOutputContext GetOutputContext(this EmitContext context)
			{
				return EmitContext.GetWriterContext(context).OutputContext;
			}
			
			/// <summary>
			/// Get the document context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the document context.</param>
			/// <returns>The document context of this context.</returns>
			public static DocumentContextType GetDocumentContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				return EmitContext<DocumentContextType, ParentEmitContextType>.GetDocumentContext(context);
			}

			/// <summary>
			/// Get the parent context from the context.
			/// </summary>
			/// <param name="context">The context from which we should get the parent context.</param>
			/// <returns>The parent of this context.</returns>
			public static ParentEmitContextType GetParentContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				return EmitContext<DocumentContextType, ParentEmitContextType>.GetParentContext(context);
			}

			
			/// <summary>
			/// Clone the emit context with a copy of the persistent data.
			/// </summary>
			/// <returns>A new emit context with a separate set of persistent data.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> ClonePersistentData<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Ensures(Contract.Result<EmitContext<DocumentContextType, ParentEmitContextType>>() != null);

				// Use ToArray instead of the IEnumerable interface to make sure we get a consistent snapshot.
				var newPersistentDataMap = new ConcurrentDictionary<object, object>(context.GetPersistentDataMap().ToArray());
				return EmitContext<DocumentContextType, ParentEmitContextType>.CopyWith(context, persistentDataMap: newPersistentDataMap);
			}

			/// <summary>
			/// Replace the emit target for this emit context.
			/// </summary>
			/// <param name="targetContext">The new target context.</param>
			/// <returns>A new emit context with the target context changed.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> ReplaceTargetContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, EmitTargetContext targetContext)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
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
			public static EmitContext<DocumentContextType, ParentEmitContextType> ReplaceTargetContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, object key, Func<EmitTargetContext> createFactory)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				return ReplaceTargetContext(context, (EmitTargetContext)context.GetPersistentDataMap().GetOrAdd(key, k => createFactory()));
			}

			/// <summary>
			/// Replace the writer context for this emit context.
			/// </summary>
			/// <param name="writerContext">The new writer context.</param>
			/// <returns>A new emit context with the writer context set to the given writer context.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> ReplaceWriterContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, EmitWriterContext writerContext)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Requires(writerContext != null);
				Contract.Ensures(Contract.Result<DocumentContextType>() != null);

				return context.GetWriterContext() != writerContext
					? EmitContext<DocumentContextType, ParentEmitContextType>.CopyWith(context, writerContext: writerContext)
					: context;
			}

			/// <summary>
			/// Replace the formatter for this emit context.
			/// </summary>
			/// <param name="formatter">The formatter to be used for this context.</param>
			/// <returns>A new emit context with an updated formatter.</returns>
			public static EmitContext<DocumentContextType, ParentEmitContextType> ReplaceFormatterContext<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, EmitFormatterContext formatter)
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
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
			public static EmitContext<NewDocumentContextType, ParentEmitContextType> 
				ReplaceDocumentContext<NewDocumentContextType, DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, NewDocumentContextType documentContext)
				where NewDocumentContextType : DocumentContext
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Requires(documentContext != null);
				Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType, EmitContext<DocumentContextType, ParentEmitContextType>>>() != null);

				return EmitContext<NewDocumentContextType, ParentEmitContextType>.CopyWith(context, context.GetParentContext(), documentContext);
			}

			/// <summary>
			/// Replace the parent context for this emit context.
			/// </summary>
			/// <param name="parentContext">The new parent context to be used for this context.</param>
			/// <returns>A new emit context with an updated parent context.</returns>
			public static EmitContext<DocumentContextType, NewParentEmitContextType>
				ReplaceParentContext<DocumentContextType, ParentEmitContextType, NewParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, NewParentEmitContextType parentContext)				
				where DocumentContextType : DocumentContext
				where ParentEmitContextType : EmitContext
				where NewParentEmitContextType : EmitContext
			{
				Contract.Requires(context != null);
				Contract.Requires(parentContext != null);
				Contract.Ensures(Contract.Result<EmitContext<DocumentContextType, NewParentEmitContextType>>() != null);

				return EmitContext<DocumentContextType, NewParentEmitContextType>.CopyWith(context, parentContext, context.GetDocumentContext());
			}
		}
	}

	// =====================================================================
	#endregion
}
