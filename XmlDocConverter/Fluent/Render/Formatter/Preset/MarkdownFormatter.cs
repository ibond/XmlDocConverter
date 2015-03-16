using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter
{
	/// <summary>
	/// Functionality for emitting markdown.
	/// </summary>
	public static class MarkdownFormatter
	{
		public static readonly EmitPreset Standard = new EmitPreset(emit => emit
			.Using(new InlineCodeFormatter.Writer((context, formatter, source) =>
				context.Write.A("`").Source(source).A("`")
			))
			.Using(new CodeFormatter.Writer((context, formatter, source) =>
				context.WithFilter(RenderFilter.Indent("    ", 1), c => c.Write.Source(source)))));

		public static readonly EmitPreset GitHub = new EmitPreset(emit => emit
			.Using(Standard)
			.Using(new CodeFormatter.Writer((context, formatter, source) =>
				context.Write.L().L("```{0}", formatter.Language).Source(source).L().L("```").L())));
	}
}
