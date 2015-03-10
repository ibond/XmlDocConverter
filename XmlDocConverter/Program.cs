using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// The XmlDocConverter program.
	/// </summary>
	class Program
	{
		/// <summary>
		/// The error codes that can be returned from the program.
		/// </summary>
		static class ErrorCodes
		{
			/// <summary>
			/// Program completed successfully.
			/// </summary>
			public static readonly int Success = 0;

			/// <summary>
			/// An unspecified error has occurred, see program output for more details.
			/// </summary>
			public static readonly int UnspecifiedError = 1;
		}


		/// <summary>
		/// The program entry point.
		/// </summary>
		/// <param name="args">The program command line arguments.</param>
		static int Main(string[] args)
		{
			try
			{
				return SubMain(args);
			}
			catch (Exception exp)
			{
				Console.Error.WriteLine("Fatal Error: {0}", exp.ToString());
				return ErrorCodes.Success;
			}
		}

		/// <summary>
		/// An inner entry point used to let us wrap the entire program in a try block.
		/// </summary>
		/// <param name="args">The program command line arguments.</param>
		static int SubMain(string[] args)
		{
			try
			{
				Converter.ConvertFromRazor(@"C:\Work\XmlDocConverter\XmlDocConverter.Tests\TestScripts\RazorTest.cshtml");
				//Converter.ConvertFromScript(@"C:\Work\XmlDocConverter\XmlDocConverter.Tests\TestScripts\BasicScript.xdc.cs");

				return ErrorCodes.Success;
			}
			catch (ScriptCompilerException exp)
			{
				Console.Error.WriteLine("Script Errors:");
				Console.Error.WriteLine(exp.ToString());
				return ErrorCodes.UnspecifiedError;
			}
		}
	}
}
