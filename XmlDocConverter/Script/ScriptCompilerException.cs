using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// This exception is thrown if there are errors in a script.
	/// </summary>
	public class ScriptCompilerException : Exception
	{
		/// <summary>
		/// Construct a ScriptCompilerException.
		/// </summary>
		/// <param name="compilerErrors">The collection of compiler errors.</param>
		public ScriptCompilerException(CompilerErrorCollection compilerErrors)
		{
			Contract.Requires(compilerErrors != null);

			CompilerErrors = compilerErrors;
		}

		/// <summary>
		/// Override the ToString function to print the compiler errors instead of printing exception information.
		/// </summary>
		/// <returns>A string containing the compiler errors.</returns>
		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (CompilerError error in CompilerErrors)
			{
				builder.AppendLine(error.ToString());
			}
			return builder.ToString();
		}

		/// <summary>
		/// The compiler errors.
		/// </summary>
		public CompilerErrorCollection CompilerErrors { get; private set; }
	}
}
