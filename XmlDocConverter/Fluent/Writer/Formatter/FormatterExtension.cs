﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is the base class for the extension system for formatters.  Each formatter extension derives from this and
	/// implements the Write function as appropriate.
	/// </summary>
	public abstract class FormatterExtension
	{
		/// <summary>
		/// Write the given value to the given writer context.
		/// </summary>
		/// <param name="writer">The writer context where the value should be written.</param>
		/// <param name="value">The value to write.</param>
		/// <returns>An updated emit writer context.</returns>
		protected abstract EmitWriterContext Write(EmitWriterContext writer, IOutputSource value);

		/// <summary>
		/// Write the given value to the emit context.
		/// </summary>
		/// <typeparam name="TDoc">The document context type of the emit context.</typeparam>
		/// <typeparam name="TParent">The parent context type of the emit context.</typeparam>
		/// <param name="context">The emit context where the value should be written.</param>
		/// <param name="value">The value to be written.</param>
		/// <returns>An updated emit context.</returns>
		public EmitContext<TDoc, TParent> Write<TDoc, TParent>(EmitContext<TDoc, TParent> context, IOutputSource value)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			return context.ReplaceWriterContext(Write(context.GetWriterContext(), value));
		}
	}
}
