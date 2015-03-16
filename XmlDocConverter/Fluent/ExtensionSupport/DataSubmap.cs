using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent.EmitContextExtensionSupport
{
	/// <summary>
	/// This provides functionality for nesting a map within an EmitContext's data maps.
	/// </summary>
	public class DataSubmap<TKey, TValue>
	{
	}


	public static class LocalDataSubmapExtensions
	{
		public static EmitContext<TDoc> UpdateLocalDataMap<TDoc, TKey, TValue>(
			this EmitContext<TDoc> context,
			DataSubmap<TKey, TValue> submap,
			Func<ImmutableDictionary<TKey, TValue>, ImmutableDictionary<TKey, TValue>> updateFunction)
			where TDoc : DocumentContext
		{
			return context.UpdateLocalDataMap(localDataMap =>
				{
					var currentMap = context.GetLocalData(submap, ImmutableDictionary<TKey, TValue>.Empty);
					return localDataMap.SetItem(submap, updateFunction(currentMap));				
				});
		}

		public static TDefaultValue GetLocalData<TDoc, TKey, TValue, TDefaultValue>(
			this EmitContext<TDoc> context,
			DataSubmap<TKey, TValue> submap,
			TKey key,
			TDefaultValue defaultValue)
			where TDoc : DocumentContext
			where TDefaultValue : TValue
		{
			return context.GetLocalData(submap, key, k => defaultValue);
		}

		public static TDefaultValue GetLocalData<TDoc, TKey, TValue, TDefaultValue>(
			this EmitContext<TDoc> context,
			DataSubmap<TKey, TValue> submap,
			TKey key,
			Func<TKey, TDefaultValue> getDefaultValue)
			where TDoc : DocumentContext
			where TDefaultValue : TValue
		{
			TValue value;
			return context.GetLocalData(submap, ImmutableDictionary<TKey, TValue>.Empty).TryGetValue(key, out value)
				? (TDefaultValue)value
				: getDefaultValue(key);
		}
	}


	public static class PersistentDataSubmapExtensions
	{
		public static ConcurrentDictionary<TKey, TValue> GetPersistentDataSubmap<TDoc, TKey, TValue>(
			this EmitContext<TDoc> context,
			DataSubmap<TKey, TValue> submap)
			where TDoc : DocumentContext
		{
			return (ConcurrentDictionary<TKey, TValue>)context.GetPersistentDataMap().GetOrAdd(submap, k => new ConcurrentDictionary<TKey, TValue>());
		}
	}
}
