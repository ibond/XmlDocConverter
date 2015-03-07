//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Contracts;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using XmlDocConverter.Fluent;

//namespace XmlDocConverter
//{
//	/// <summary>
//	/// The EmitItem for an Assembly.
//	/// </summary>
//	public class AssemblyItem : EmitItem
//	{
//		/// <summary>
//		/// Construct an AssemblyItem.
//		/// </summary>
//		/// <param name="assembly">The Assembly contained within this item.</param>
//		/// <param name="context">The emit context associated with this source item.</param>
//		public AssemblyItem(Assembly assembly, EmitContext<AssemblyItem> context)
//		{
//			Contract.Requires(assembly != null);
//			Contract.Ensures(this.m_assembly != null);

//			m_assembly = assembly;
//			m_context = context;
//		}

//		/// <summary>
//		/// Gets the name of this assembly.
//		/// </summary>
//		public string Name { get { return m_assembly.GetName().Name; } }

//		/// <summary>
//		/// Gets the assembly for this item.
//		/// </summary>
//		public Assembly Assembly { get { return m_assembly; } }

//		/// <summary>
//		/// The Assembly for this AssemblyItem.
//		/// </summary>
//		private readonly Assembly m_assembly;

//		/// <summary>
//		/// The emit context for this document source.
//		/// </summary>
//		private readonly EmitContext<AssemblyItem> m_context;
//	}
//}
