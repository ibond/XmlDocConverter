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
	/// The emit item group contains a list of emit contexts.
	/// </summary>
	/// <typeparam name="ItemType">The type of the EmitItem of the EmitContext contained in this group.</typeparam>
	/// <typeparam name="SourceItemType">The type of the EmitItem of the EmitContext that created this item.</typeparam>
	public class EmitContextGroup<ItemType, SourceItemType> : IEnumerable<EmitContext<ItemType>>
		where ItemType : EmitItem
		where SourceItemType : EmitItem
	{
		/// <summary>
		/// Construct an EmitItemGroup.
		/// </summary>
		/// <param name="entries">The items contained within this list.</param>
		/// <param name="sourceContext">The context from which this group was derived.</param>
		public EmitContextGroup(ImmutableList<EmitContext<ItemType>> entries, EmitContext<SourceItemType> sourceContext)
		{
			Contract.Requires(entries != null);
			Contract.Ensures(this.m_entries != null);

			m_entries = entries;
			m_sourceContext = sourceContext;
		}

		/// <summary>
		/// Call the action for each of the emit items.
		/// </summary>
		/// <param name="action">The action to be called for each of the emit items in this group.</param>
		/// <returns>The source EmitContext.</returns>
		public EmitContext<SourceItemType> ForEach(Action<EmitContext<ItemType>> action)
		{
			foreach (var entry in m_entries)
			{
				action(entry);
			}

			return m_sourceContext;
		}

		/// <summary>
		/// Implement IEnumerable&lt;ItemType&gt;.GetEnumerator.
		/// </summary>
		/// <returns>An enumerator over the items in this group.</returns>
		public IEnumerator<EmitContext<ItemType>> GetEnumerator()
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
		private readonly ImmutableList<EmitContext<ItemType>> m_entries;

		/// <summary>
		/// The source context for this group.
		/// </summary>
		private readonly EmitContext<SourceItemType> m_sourceContext;
	}
}
