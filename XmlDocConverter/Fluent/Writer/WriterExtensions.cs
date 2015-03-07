﻿using System;
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
	public static class WriterExtensions
	{
		/// <summary>
		/// Replace the formatter for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<DocumentContextType, ParentEmitContextType> Using<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, EmitFormatterContext formatter)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			return context.ReplaceFormatterContext(formatter);
		}
	}
}