using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	public abstract class WriteExtension
	{
		protected abstract EmitWriterContext Write(EmitWriterContext context, TextWriter writer, string value);

		public EmitContext<ItemType> Write<ItemType>(EmitContext<ItemType> context, string value)
			where ItemType : EmitItem
		{
			return context.ReplaceEmitWriter(Write(context.EmitWriter, context.TextWriter, value));
		}
	}
}
