using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Util;

namespace XmlDocConverter.Fluent
{
	public class CodeFormatter : Formatter<CodeFormatter>
	{
		public readonly string Language = "C#";
	}

	public static class CodeFormatterExtensions
	{
		public static EmitWriteContext<TDoc> Code<TDoc>(this EmitWriteContext<TDoc> context, FormatterContentSource source)
			where TDoc : DocumentContext
		{
			return Formatter.Format<CodeFormatter, TDoc>(context, source);
		}
	}


	public class InlineCodeFormatter : Formatter<InlineCodeFormatter>
	{
	}
	
	public static class InlineCodeFormatterExtensions
	{
		public static EmitWriteContext<TDoc> InlineCode<TDoc>(this EmitWriteContext<TDoc> context, FormatterContentSource source)
			where TDoc : DocumentContext
		{
			return Formatter.Format<InlineCodeFormatter, TDoc>(context, source);
		}
	}
}
