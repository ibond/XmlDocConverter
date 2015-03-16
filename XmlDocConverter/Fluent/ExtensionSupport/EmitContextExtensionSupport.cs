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
		public static EmitContext<TDoc>
			UpdateLocalDataMap<TDoc>(
				this EmitContext<TDoc> context,
				Func<ImmutableDictionary<object, object>, ImmutableDictionary<object, object>> updateFunction)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(updateFunction != null);
			Contract.Ensures(Contract.Result<TDoc>() != null);

			return context.ReplaceLocalDataMap(updateFunction(context.GetLocalDataMap()));
		}

		/// <summary>
		/// Replace the local data map for this emit context.
		/// </summary>
		/// <param name="localDataMap">The new local data map.</param>
		/// <returns>A new emit context with an updated local data map.</returns>
		public static EmitContext<TDoc> 
			ReplaceLocalDataMap<TDoc>(
				this EmitContext<TDoc> context, 
				ImmutableDictionary<object, object> localDataMap)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(localDataMap != null);
			Contract.Ensures(Contract.Result<TDoc>() != null);

			return context.GetLocalDataMap() != localDataMap
				? EmitContext<TDoc>.CopyWith(context, localDataMap: localDataMap)
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
		/// Get the output context from the context.
		/// </summary>
		/// <param name="context">The context from which we should get the output context.</param>
		/// <returns>The output context of this context.</returns>
		public static EmitOutputContext GetOutputContext(this EmitContext context)
		{
			return EmitContext.GetOutputContext(context);
		}
		
		/// <summary>
		/// Get the document context from the context.
		/// </summary>
		/// <param name="context">The context from which we should get the document context.</param>
		/// <returns>The document context of this context.</returns>
		public static TDoc GetDocumentContext<TDoc>(this EmitContext<TDoc> context)
			where TDoc : DocumentContext
		{
			return EmitContext<TDoc>.GetDocumentContext(context);
		}
		
		/// <summary>
		/// Clone the emit context with a copy of the persistent data.
		/// </summary>
		/// <returns>A new emit context with a separate set of persistent data.</returns>
		public static EmitContext<TDoc> ClonePersistentData<TDoc>(this EmitContext<TDoc> context)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<EmitContext<TDoc>>() != null);

			// Use ToArray instead of the IEnumerable interface to make sure we get a consistent snapshot.
			var newPersistentDataMap = new ConcurrentDictionary<object, object>(context.GetPersistentDataMap().ToArray());
			return EmitContext<TDoc>.CopyWith(context, persistentDataMap: newPersistentDataMap);
		}

		/// <summary>
		/// Replace the emit target for this emit context.
		/// </summary>
		/// <param name="targetContext">The new target context.</param>
		/// <returns>A new emit context with the target context changed.</returns>
		public static EmitContext<TDoc> ReplaceTargetContext<TDoc>(this EmitContext<TDoc> context, EmitTargetContext targetContext)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(targetContext != null);
			Contract.Ensures(Contract.Result<TDoc>() != null);

			// Register the target context.
			Script.CurrentRunContext.RegisterEmitTarget(targetContext);

			// Update the writer context.
			return context.ReplaceOutputContext(targetContext.OutputContext);
		}

		/// <summary>
		/// Get the existing EmitTargetContext from the data map using the given key, or if it doesn't exist call
		/// createFactory to create one and add it to the data map.  Replace the emit target for this emit context with
		/// the result.
		/// </summary>
		/// <param name="key">The key for this target.</param>
		/// <param name="createFunction">The EmitTargetContext factory function.</param>
		/// <returns>A new emit context with the target context changed.</returns>
		public static EmitContext<TDoc> ReplaceTargetContext<TDoc>(this EmitContext<TDoc> context, object key, Func<EmitTargetContext> createFactory)
			where TDoc : DocumentContext
		{
			return ReplaceTargetContext(context, context.GetPersistentDataSubmap(EmitTargetSubmap).GetOrAdd(key, k => createFactory()));
		}

		private static DataSubmap<object, EmitTargetContext> EmitTargetSubmap = new DataSubmap<object, EmitTargetContext>();


		/// <summary>
		/// Replace the output context for this emit context.
		/// </summary>
		/// <param name="output">The output context to be used for this context.</param>
		/// <returns>A new emit context with an updated output context.</returns>
		public static EmitContext<TDoc> ReplaceOutputContext<TDoc>(this EmitContext<TDoc> context, EmitOutputContext outputContext)
			where TDoc : DocumentContext
		{
			return context.GetOutputContext() != outputContext
				? EmitContext<TDoc>.CopyWith(context, outputContext: outputContext)
				: context;
		}

		/// <summary>
		/// Replace the document context for this emit context.
		/// </summary>
		/// <param name="documentContext">The new document context to be used for this context.</param>
		/// <returns>A new emit context with an updated document context.</returns>
		public static EmitContext<TDoc>	ReplaceDocumentContext<TDoc>(this EmitContext context, TDoc documentContext)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(documentContext != null);
			Contract.Ensures(Contract.Result<EmitContext<TDoc>>() != null);

			return EmitContext.CopyWith(context, documentContext);
		}
	}
}
