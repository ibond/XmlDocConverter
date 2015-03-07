using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	//public static class WriteHeaderExtension
	//{
	//	class Impl : WriteExtension
	//	{
	//		public static readonly Impl Default = new Impl(1);

	//		protected override EmitFormatterContext Write(EmitFormatterContext context, TextWriter writer, string value)
	//		{
	//			writer.WriteLine("{0} {1}", new string('#', Math.Min(6, Math.Max(1, HeaderLevel))), value);
	//			return context;
	//		}

	//		public readonly int HeaderLevel;

	//		public Impl(int headerLevel)
	//		{
	//			HeaderLevel = headerLevel;
	//		}
	//	}

	//	public static EmitContext<ItemType> WriteHeader<ItemType>(this EmitContext<ItemType> context, string value)
	//		where ItemType : EmitItem
	//	{
	//		return context.EmitWriter.GetWriteExtension(Impl.Default).Write(context, value);
	//	}
		
	//	public static EmitContext<ItemType> SetHeaderLevel<ItemType>(this EmitContext<ItemType> context, int headerLevel)
	//		where ItemType : EmitItem
	//	{
	//		return context.ReplaceEmitWriter(context.EmitWriter.SetWriteExtension(new Impl(headerLevel)));
	//	}
	//}
}
