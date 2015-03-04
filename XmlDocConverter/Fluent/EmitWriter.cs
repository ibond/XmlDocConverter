using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This type contains the context for how to emit documentation.
	/// </summary>
	public class EmitWriterContext
	{
		public readonly Func<EmitContext<AssemblyItem>, AssemblyItem, EmitContext<AssemblyItem>> WriteAssembly =
			(emit, item) =>
			{
				return emit;
			};

		public readonly Func<EmitContext<ClassItem>, ClassItem, EmitContext<ClassItem>> WriteClass =
			(emit, item) =>
			{
				return emit;
			};

		public readonly Func<EmitContext<EnumItem>, EnumItem, EmitContext<EnumItem>> WriteEnum =
			(emit, item) =>
			{
				return emit;
			};
	}
}
