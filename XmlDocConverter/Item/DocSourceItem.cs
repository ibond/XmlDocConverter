//using NuDoq;
//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics.Contracts;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using XmlDocConverter.Fluent;

//namespace XmlDocConverter
//{
//	/// <summary>
//	/// The EmitItem for a document source.  This is the initial item type used when creating an emitter.
//	/// </summary>
//	public class DocSourceItem : EmitItem
//	{
//		/// <summary>
//		/// Construct an DocSourceItem.
//		/// </summary>
//		/// <param name="documentSource">The assembly member sources to be used for this context.</param>
//		/// <param name="context">The emit context associated with this item.</param>
//		public DocSourceItem(ImmutableList<AssemblyMembers> documentSource, EmitContext<DocSourceItem> context)
//		{
//			Contract.Requires(documentSource != null);
//			Contract.Requires(Contract.ForAll(documentSource, d => d != null));
//			Contract.Ensures(this.m_documentSource != null);

//			m_documentSource = documentSource;
//			m_context = context;
//		}
		
//		/// <summary>
//		/// Groups the items in the EmitContext by Assembly.
//		/// </summary>
//		public EmitContextGroup<AssemblyItem, DocSourceItem> Assemblies
//		{
//			get
//			{
//				return new EmitContextGroup<AssemblyItem, DocSourceItem>(
//					m_documentSource
//						.Select(
//							source => new EmitContext<AssemblyItem>(
//								m_context, 
//								new LazyEmitItem<AssemblyItem>(context=>new AssemblyItem(source.Assembly, context))))
//						.ToImmutableList(),
//					m_context);
//			}
//		}
		
//		/// <summary>
//		/// The assembly members for this doc source.
//		/// </summary>
//		private readonly ImmutableList<AssemblyMembers> m_documentSource;

//		/// <summary>
//		/// The emit context for this item.
//		/// </summary>
//		private readonly EmitContext<DocSourceItem> m_context;
//	}
//}
