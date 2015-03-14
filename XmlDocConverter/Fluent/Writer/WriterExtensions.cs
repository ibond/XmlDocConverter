using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// Extension methods to make using writers more fluent.
	/// </summary>
	public static class WriterExtensionsX
	{
		/// <summary>
		/// Replace the formatter for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<TDoc> Using<TDoc>(this EmitContext<TDoc> context, EmitFormatterContext formatter)
			where TDoc : DocumentContext
		{
			return context.ReplaceFormatterContext(formatter);
		}
	}
}
