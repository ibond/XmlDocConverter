using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public static class WriteHeaderExtension
	{
		public class Impl : FormatterExtension
		{			
			protected override EmitWriterContext Write(EmitWriterContext writer, string value)
			{
				writer.OutputContext.WriteLine("{0} {1}", new string('#', Math.Min(6, Math.Max(1, HeaderLevel))), value);
				return writer;
			}

			public readonly int HeaderLevel;

			public Impl(int headerLevel)
			{
				HeaderLevel = headerLevel;
			}
		}

		public static readonly Impl Default = new Impl(1);

		public static EmitContext<TDocContext, TParentContext> WriteHeader<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, string value)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return context.GetWriterContext().FormatterContext.GetFormatterExtension(Default).Write(context, value);
		}
	}
}
