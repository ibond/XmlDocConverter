using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	public struct FormatterContentSource
	{
		private FormatterContentSource(IOutputSource source)
		{
			OutputSource = source;
		}

		public static implicit operator FormatterContentSource(string source)
		{
			return new FormatterContentSource(new StringOutputSource(source));
		}

		public readonly IOutputSource OutputSource;
	}
}
