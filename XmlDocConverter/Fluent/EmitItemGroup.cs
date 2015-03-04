using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A single item entry in an emit group.
	/// </summary>
	/// <typeparam name="ItemType">The type of the EmitItem contained in this entry.</typeparam>
	public struct EmitItemGroupEntry<ItemType>
		where ItemType : EmitItem
	{
		/// <summary>
		/// Construct an EmitItemGroupEntry.
		/// </summary>
		/// <param name="item">The item for this entry.</param>
		/// <param name="context">The context for this entry.</param>
		public EmitItemGroupEntry(ItemType item, EmitContext<ItemType> context)
		{
			Contract.Requires(item != null);
			Contract.Ensures(this.Item != null);

			Item = item;
			Context = context;
		}

		/// <summary>
		/// The emit item for this entry.
		/// </summary>
		public readonly ItemType Item;

		/// <summary>
		/// The emit context for this entry.
		/// </summary>
		public readonly EmitContext<ItemType> Context;
	}

	/// <summary>
	/// The emit item group contains a list of emit items.
	/// </summary>
	/// <typeparam name="ItemType">The type of the EmitItem contained in this group.</typeparam>
	/// <typeparam name="SourceItemType">The type of the EmitItem of the EmitContext that created this item.</typeparam>
	public class EmitItemGroup<ItemType, SourceItemType> : IEnumerable<EmitItemGroupEntry<ItemType>>
		where ItemType : EmitItem
		where SourceItemType : EmitItem
	{
		/// <summary>
		/// Construct an EmitItemGroup.
		/// </summary>
		/// <param name="entries">The items contained within this list.</param>
		/// <param name="context">The context from which this group was derived.</param>
		public EmitItemGroup(ImmutableList<EmitItemGroupEntry<ItemType>> entries, EmitContext<SourceItemType> context)
		{
			Contract.Requires(entries != null);
			Contract.Requires(Contract.ForAll(entries, e => e.Item != null));
			Contract.Ensures(this.m_entries != null);

			m_entries = entries;
			m_sourceContext = context;
		}

		/// <summary>
		/// Call the action for each of the emit items.
		/// </summary>
		/// <param name="action">The action to be called for each of the emit items in this group.</param>
		/// <returns>The source EmitContext.</returns>
		public EmitContext<SourceItemType> ForEach(Action<EmitContext<ItemType>, ItemType> action)
		{
			foreach (var entry in m_entries)
			{
				action(entry.Context, entry.Item);
			}

			return m_sourceContext;
		}

		/// <summary>
		/// Implement IEnumerable&lt;ItemType&gt;.GetEnumerator.
		/// </summary>
		/// <returns>An enumerator over the items in this group.</returns>
		public IEnumerator<EmitItemGroupEntry<ItemType>> GetEnumerator()
		{
			return m_entries.GetEnumerator();
		}

		/// <summary>
		/// Implement IEnumerable.GetEnumerator.
		/// </summary>
		/// <returns>An enumerator over the items in this group.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Get the emit context for this group.
		/// </summary>
		public EmitContext<SourceItemType> Context { get { return m_sourceContext; } }

		/// <summary>
		/// The items in this group.
		/// </summary>
		private readonly ImmutableList<EmitItemGroupEntry<ItemType>> m_entries;

		/// <summary>
		/// The source context for this group.
		/// </summary>
		private readonly EmitContext<SourceItemType> m_sourceContext;
	}
}
