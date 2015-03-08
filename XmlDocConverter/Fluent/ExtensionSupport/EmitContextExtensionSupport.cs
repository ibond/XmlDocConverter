using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent.EmitContextExtensionSupport
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
		/// Update the local data map for this emit context.
		/// </summary>
		/// <param name="updateFunction">The function used to update the data map.  This takes the existing data map and returns a new data map.</param>
		/// <returns>A new emit context with an updated local data map.</returns>
		public static EmitContext<TDocContext, TParentContext>
			UpdateLocalDataMap<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context,
				Func<ImmutableDictionary<object, object>, ImmutableDictionary<object, object>> updateFunction)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(updateFunction != null);
			Contract.Ensures(Contract.Result<TDocContext>() != null);

			return context.ReplaceLocalDataMap(updateFunction(context.GetLocalDataMap()));
		}

		/// <summary>
		/// Replace the local data map for this emit context.
		/// </summary>
		/// <param name="localDataMap">The new local data map.</param>
		/// <returns>A new emit context with an updated local data map.</returns>
		public static EmitContext<TDocContext, TParentContext> 
			ReplaceLocalDataMap<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context, 
				ImmutableDictionary<object, object> localDataMap)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(localDataMap != null);
			Contract.Ensures(Contract.Result<TDocContext>() != null);

			return context.GetLocalDataMap() != localDataMap
				? EmitContext<TDocContext, TParentContext>.CopyWith(context, localDataMap: localDataMap)
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
		public static TDocContext GetDocumentContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return EmitContext<TDocContext, TParentContext>.GetDocumentContext(context);
		}

		/// <summary>
		/// Get the parent context from the context.
		/// </summary>
		/// <param name="context">The context from which we should get the parent context.</param>
		/// <returns>The parent of this context.</returns>
		public static TParentContext GetParentContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return EmitContext<TDocContext, TParentContext>.GetParentContext(context);
		}


		/// <summary>
		/// Clone the emit context with a copy of the persistent data.
		/// </summary>
		/// <returns>A new emit context with a separate set of persistent data.</returns>
		public static EmitContext<TDocContext, TParentContext> ClonePersistentData<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<EmitContext<TDocContext, TParentContext>>() != null);

			// Use ToArray instead of the IEnumerable interface to make sure we get a consistent snapshot.
			var newPersistentDataMap = new ConcurrentDictionary<object, object>(context.GetPersistentDataMap().ToArray());
			return EmitContext<TDocContext, TParentContext>.CopyWith(context, persistentDataMap: newPersistentDataMap);
		}

		/// <summary>
		/// Replace the emit target for this emit context.
		/// </summary>
		/// <param name="targetContext">The new target context.</param>
		/// <returns>A new emit context with the target context changed.</returns>
		public static EmitContext<TDocContext, TParentContext> ReplaceTargetContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, EmitTargetContext targetContext)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(targetContext != null);
			Contract.Ensures(Contract.Result<TDocContext>() != null);

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
		public static EmitContext<TDocContext, TParentContext> ReplaceTargetContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, object key, Func<EmitTargetContext> createFactory)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return ReplaceTargetContext(context, (EmitTargetContext)context.GetPersistentDataMap().GetOrAdd(key, k => createFactory()));
		}

		/// <summary>
		/// Replace the writer context for this emit context.
		/// </summary>
		/// <param name="writerContext">The new writer context.</param>
		/// <returns>A new emit context with the writer context set to the given writer context.</returns>
		public static EmitContext<TDocContext, TParentContext> ReplaceWriterContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, EmitWriterContext writerContext)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(writerContext != null);
			Contract.Ensures(Contract.Result<TDocContext>() != null);

			return context.GetWriterContext() != writerContext
				? EmitContext<TDocContext, TParentContext>.CopyWith(context, writerContext: writerContext)
				: context;
		}

		/// <summary>
		/// Replace the formatter for this emit context.
		/// </summary>
		/// <param name="formatter">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<TDocContext, TParentContext> ReplaceFormatterContext<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, EmitFormatterContext formatter)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
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
		public static EmitContext<NewDocumentContextType, TParentContext>
			ReplaceDocumentContext<NewDocumentContextType, TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, NewDocumentContextType documentContext)
			where NewDocumentContextType : DocumentContext
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType, EmitContext<TDocContext, TParentContext>>>() != null);

			return EmitContext<NewDocumentContextType, TParentContext>.CopyWith(context, documentContext, context.GetParentContext());
		}

		/// <summary>
		/// Replace the document context for this emit context.
		/// </summary>
		/// <param name="documentContext">The new document context to be used for this context.</param>
		/// <returns>A new emit context with an updated document context.</returns>
		public static EmitContext<NewDocumentContextType>
			ReplaceDocumentContext<NewDocumentContextType, TDocContext>(this EmitContext<TDocContext> context, NewDocumentContextType documentContext)
			where NewDocumentContextType : DocumentContext
			where TDocContext : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<NewDocumentContextType, EmitContext<TDocContext>>>() != null);

			return EmitContext<NewDocumentContextType>.CopyWith(context, documentContext, context.GetParentContext());
		}

		/// <summary>
		/// Replace the document context for this emit context.
		/// </summary>
		/// <param name="documentContext">The new document context to be used for this context.</param>
		/// <returns>A new emit context with an updated document context.</returns>
		public static EmitContext<TDocContext, TParentContext>
			ReplaceDocumentAndParentContext<TDocContext, TParentContext>(this EmitContext context, TDocContext documentContext, TParentContext parentContext)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDocContext, TParentContext>>() != null);

			return EmitContext<TDocContext, TParentContext>.CopyWith(context, documentContext, parentContext);
		}

		/// <summary>
		/// Replace the parent context for this emit context.
		/// </summary>
		/// <param name="parentContext">The new parent context to be used for this context.</param>
		/// <returns>A new emit context with an updated parent context.</returns>
		public static EmitContext<TDocContext, NewParentEmitContextType>
			ReplaceParentContext<TDocContext, TParentContext, NewParentEmitContextType>(this EmitContext<TDocContext, TParentContext> context, NewParentEmitContextType parentContext)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
			where NewParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(parentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDocContext, NewParentEmitContextType>>() != null);

			return EmitContext<TDocContext, NewParentEmitContextType>.CopyWith(context, context.GetDocumentContext(), parentContext);
		}

		/// <summary>
		/// Replace the parent context for this emit context.
		/// </summary>
		/// <param name="parentContext">The new parent context to be used for this context.</param>
		/// <returns>A new emit context with an updated parent context.</returns>
		public static EmitContext<TDocContext, NewParentEmitContextType>
			ReplaceParentContext<TDocContext, NewParentEmitContextType>(this EmitContext<TDocContext> context, NewParentEmitContextType parentContext)
			where TDocContext : DocumentContext
			where NewParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(parentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDocContext, NewParentEmitContextType>>() != null);

			return EmitContext<TDocContext, NewParentEmitContextType>.CopyWith(context, context.GetDocumentContext(), parentContext);
		}
	}
}
