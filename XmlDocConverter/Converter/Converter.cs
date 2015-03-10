using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
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
		public static void ConvertFromRazor(string razorPath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(razorPath));

			
			var script = new Script(() =>
			{
				// The text of the initial razor file.
				var fileText = File.ReadAllText(razorPath);

				// The configuration.
				var config = new TemplateServiceConfiguration();

				// Enable debugging.
				config.Debug = true;

				// Use the raw string encoder.
				config.EncodedStringFactory = new RawStringFactory();
				
				// Create the service.
				Engine.Razor = RazorEngineService.Create(config);

				var result = Engine.Razor.RunCompile(new LoadedTemplateSource(fileText, razorPath), razorPath);
				var x = result;
			});

			script.Run();
		}

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
			"System.Linq.dll",
			"RazorEngine.dll"
		};
	}
}
