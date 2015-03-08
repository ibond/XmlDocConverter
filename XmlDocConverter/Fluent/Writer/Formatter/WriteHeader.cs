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
	public class HeaderFormatter : FormatterExtension
	{
		public HeaderFormatter(int headerLevel)
		{
			HeaderLevel = headerLevel;
			Func<IOutputSource, HeaderFormatter, IOutputSource> x = (source, formatter) =>
				source;
		}

		public readonly int HeaderLevel;

		protected override EmitWriterContext Write(EmitWriterContext writer, IOutputSource value)
		{
			writer.OutputContext.WriteLine("{0} {1}", new string('#', Math.Min(6, Math.Max(1, HeaderLevel))), value);
			return writer;
		}
	}

	public static class WriteHeaderExtension2
	{
		public static readonly HeaderFormatter Default = new HeaderFormatter(1);

		public static EmitContext<TDoc, TParent> WriteHeader<TDoc, TParent>(this EmitContext<TDoc, TParent> context, IOutputSource source)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			return context.GetWriterContext().FormatterContext.GetFormatterExtension(Default).Write(context, source);
		}

		public static EmitContext<TDoc, TParent> IndentHeader<TDoc, TParent>(this EmitContext<TDoc, TParent> context)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			return context.UpdateFormatterExtension(Default, f => new HeaderFormatter(f.HeaderLevel + 1));
		}

		public static EmitContext<TDoc, TParent> DedentHeader<TDoc, TParent>(this EmitContext<TDoc, TParent> context)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			return context.UpdateFormatterExtension(Default, f => new HeaderFormatter(f.HeaderLevel - 1));
		}
	}
}
