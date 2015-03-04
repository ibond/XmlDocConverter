using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;

namespace XmlDocConverter
{
	/// <summary>
	/// The EmitItem for an Enum.
	/// </summary>
	public class EnumItem : EmitItem
	{
		/// <summary>
		/// Construct an AssemblyItem.
		/// </summary>
		/// <param name="assembly">The Assembly contained within this item.</param>
		/// <param name="context">The EmitContext containing only the items in the assembly.</param>
		public EnumItem(Assembly assembly)
		{
			Contract.Requires(assembly != null);
			Contract.Ensures(this.m_assembly != null);

			m_assembly = assembly;
		}

		/// <summary>
		/// Gets the name of this assembly.
		/// </summary>
		public string Name { get { return m_assembly.GetName().Name; } }

		/// <summary>
		/// Gets the assembly for this item.
		/// </summary>
		public Assembly Assembly { get { return m_assembly; } }

		/// <summary>
		/// The Assembly for this AssemblyItem.
		/// </summary>
		private readonly Assembly m_assembly;
	}
}
