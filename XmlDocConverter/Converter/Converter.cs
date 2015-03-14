using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// Top-level methods for starting the conversion process.
	/// </summary>
	public static class Converter
	{
		/// <summary>
		/// Convert the document from a script file.
		/// </summary>
		/// <param name="scriptPath">The path to the script file.</param>
		public static void ConvertFromScript(string scriptPath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(scriptPath));

			// Create the compiler options.
			var options = new ScriptCompilerOptions();
			options.ModuleReferences.AddRange(ForcedModuleReferences);

			// Load and run the script.
			ScriptCompiler.Load(scriptPath, options).Run();
		}

		/// <summary>
		/// The list of forced module references.
		/// </summary>
		private static IEnumerable<string> ForcedModuleReferences = new string[]
		{
			// Add in this assembly since it contains all of the types needed by the script.
			Assembly.GetExecutingAssembly().Location,

			// Add some standard assemblies.
			"System.dll",
			"System.Core.dll",
			"System.Linq.dll"
		};
	}
}
