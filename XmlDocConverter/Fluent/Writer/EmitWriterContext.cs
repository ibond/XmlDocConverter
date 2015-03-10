using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{	
	/// <summary>
	/// The writer context is a combination of output context and formatter context.  This defines how the output should
	/// look and where it should go.
	/// </summary>
	public class EmitWriterContext
	{
		/// <summary>
		/// Construct a writer context using the default formatter.
		/// </summary>
		public EmitWriterContext()
			:this(new EmitOutputContext(), EmitFormatterContext.Default)
		{
		}

		/// <summary>
		/// Construct a new writer context with the given output and formatter contexts.
		/// </summary>
		/// <param name="outputContext">The output context to use for this writer.</param>
		/// <param name="formatterContext">The formatter context to use for this writer.</param>
		private EmitWriterContext(EmitOutputContext outputContext, EmitFormatterContext formatterContext)
		{
			Contract.Requires(outputContext != null);
			Contract.Requires(formatterContext != null);
			Contract.Ensures(OutputContext != null);
			Contract.Ensures(FormatterContext != null);

			OutputContext = outputContext;
			FormatterContext = formatterContext;
		}

		/// <summary>
		/// Create a new writer context based on the existing context.
		/// </summary>
		/// <returns>A new writer context with an empty output context and a fresh formatter context.</returns>
		public EmitWriterContext CreateNew()
		{
			Contract.Requires(Contract.Result<EmitWriterContext>() != null);

			// TODO: Reset the formatter context.
			return new EmitWriterContext(new EmitOutputContext(), FormatterContext);
		}

		/// <summary>
		/// Replace the formatter for this writer context.
		/// </summary>
		/// <param name="formatter">The formatter to be used for this context.</param>
		/// <returns>A new writer context with an updated formatter.</returns>
		public EmitWriterContext ReplaceFormatterContext(EmitFormatterContext formatter)
		{
			Contract.Requires(formatter != null);
			Contract.Requires(Contract.Result<EmitWriterContext>() != null);

			return FormatterContext != formatter
				? new EmitWriterContext(OutputContext, formatter)
				: this;
		}

		/// <summary>
		/// Replace the output context for this writer context.
		/// </summary>
		/// <param name="formatter">The output context to be used for this context.</param>
		/// <returns>A new writer context with an updated output context.</returns>
		public EmitWriterContext ReplaceOutputContext(EmitOutputContext output)
		{
			Contract.Requires(output != null);
			Contract.Requires(Contract.Result<EmitWriterContext>() != null);

			return OutputContext != output
				? new EmitWriterContext(output, FormatterContext)
				: this;
		}

		/// <summary>
		/// The output context for this target.
		/// </summary>
		public readonly EmitOutputContext OutputContext;

		/// <summary>
		/// The formatter context.
		/// </summary>
		public readonly EmitFormatterContext FormatterContext;
	}
}
