using NuDoq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;

namespace XmlDocConverter
{
	/// <summary>
	/// The EmitItem for a document source.  This is the initial item type used when creating an emitter.
	/// </summary>
	public class DocSourceItem : EmitItem
	{
		/// <summary>
		/// Construct an DocSourceItem.
		/// </summary>
		/// <param name="documentSource">The assembly member sources to be used for this context.</param>
		public DocSourceItem(ImmutableList<AssemblyMembers> documentSource)
		{
			Contract.Requires(documentSource != null);
			Contract.Requires(Contract.ForAll(documentSource, d => d != null));
			Contract.Ensures(this.m_documentSource != null);

			m_documentSource = documentSource;
		}
		
		/// <summary>
		/// Groups the items in the EmitContext by Assembly.
		/// </summary>
		//public EmitItemGroup<AssemblyItem, DocSourceItem> Assemblies
		//{
		//	get
		//	{			
		//		return new EmitItemGroup<AssemblyItem, DocSourceItem>(
		//			m_documentSource
		//				.Select(
		//					source =>
		//					{
		//						var item = new AssemblyItem(
		//							source.Assembly,
		//							EmitContext<DocSourceItem>.MakeContext<AssemblyItem>(m_context, item));
		//						return new EmitItemGroupEntry<AssemblyItem>(
		//							item,
		//							EmitContext<DocSourceItem>.MakeContext<AssemblyItem>(m_context, item, new AssemblyMembers[] { source }.ToImmutableList()));
		//					})
		//				.ToImmutableList(),
		//			m_context);
		//	}
		//}
		
		/// <summary>
		/// The assembly members for this doc source.
		/// </summary>
		private readonly ImmutableList<AssemblyMembers> m_documentSource;
	}
}
