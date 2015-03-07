using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;

namespace XmlDocConverter
{
	/// <summary>
	/// Functionality for emitting markdown.
	/// </summary>
	public static class MarkdownEmitWriter
	{
		public static readonly EmitFormatterContext GitHub = new EmitFormatterContext();
		//public void Write(EmitContext emit, AssemblyItem item)
		//{
		//	Contract.Requires(item != null);

		//	WriteHeader(target, item.Name);
		//}

		//private void WriteHeader(TextWriter target, string text)
		//{
		//	Contract.Requires(target != null);
		//	Contract.Requires(text != null);

		//	target.WriteLine(@"# {0}", text);
		//	target.WriteLine();			
		//}
	}
}
