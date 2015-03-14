using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Util
{
	/// <summary>
	/// This maps a set of objects that are unique within the context of this map to a set of objects that are globally
	/// unique across the entire process.  This is used so that multiple unrelated components can share a single
	/// dictionary without any sort of coordination.
	/// </summary>
	public class UniqueKeyMap<T>
	{
		/// <summary>
		/// Get the unique object for the given object.
		/// </summary>
		/// <param name="index">The locally unique object.</param>
		/// <returns>A globally unique object.</returns>
		public object this[T key]
		{
			get
			{
				// Create a new object if this one doesn't exist.
				return m_map.GetOrAdd(key, i => new object());
			}
		}

		/// <summary>
		/// Remove the given key from the map.
		/// </summary>
		/// <param name="key">The key to be removed from the map.</param>
		/// <returns>true if the key existed and was removed, false if the key did not exist in the map.</returns>
		public bool Remove(T key)
		{
			object dummy;
			return m_map.TryRemove(key, out dummy);
		}

		/// <summary>
		/// The map of input objects to object keys.
		/// </summary>
		private readonly ConcurrentDictionary<T, object> m_map = new ConcurrentDictionary<T, object>();
	}
}
