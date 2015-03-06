using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This class lazily creates an emit item.  It's similar to the built in .NET Lazy, but the creation function takes
	/// an EmitContext parameter.
	/// </summary>
	public class LazyEmitItem<ItemType>
		where ItemType : EmitItem
	{
		/// <summary>
		/// Construct a LazyEmitItem.
		/// </summary>
		/// <param name="initFunc">The function used to create the item.</param>
		public LazyEmitItem(Func<EmitContext<ItemType>, ItemType> initFactory)
		{
			Contract.Requires(initFactory != null);
			Contract.Ensures(this.m_initFactory != null);

			m_initFactory = initFactory;
		}

		/// <summary>
		/// Get the item.
		/// </summary>
		/// <param name="context">The context to be used to create the item.</param>
		public ItemType GetItem(EmitContext<ItemType> context)
		{
			Contract.Ensures(this.m_cachedItem != null);
			Contract.Ensures(Contract.Result<ItemType>() != null);

			if (m_cachedItem == null)
				m_cachedItem = m_initFactory(context);

			return m_cachedItem;
		}

		/// <summary>
		/// The init factory function.
		/// </summary>
		private readonly Func<EmitContext<ItemType>, ItemType> m_initFactory;

		/// <summary>
		/// The cached init item.  Created the first time the item is accessed.
		/// </summary>
		private ItemType m_cachedItem;
	}
}
