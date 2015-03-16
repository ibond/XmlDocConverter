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
	public static class HeaderFormatter
	{
		class Data : Formatter<Data>
		{
			public readonly int HeaderLevel = 1;
		}

		public static EmitWriteContext<TDoc> Header<TDoc>(this EmitWriteContext<TDoc> context, FormatterContentSource source)
			where TDoc : DocumentContext
		{
			return Formatter.Format<Data, TDoc>(context, source);
		}
	}
}
