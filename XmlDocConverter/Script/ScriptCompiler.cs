using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// This class compiles converter scripts.
	/// </summary>
	public static class ScriptCompiler
	{
		/// <summary>
		/// Compile and load the script at the given source file.
		/// </summary>
		/// <param name="sourceFile">The path to the script.</param>
		/// <param name="options">The script compiler options.  The default options will be used if this parameter is null.</param>
		/// <returns>A Script object that can be used to run the loaded script.</returns>
		public static Script Load(string sourceFile, ScriptCompilerOptions options = null)
		{
			Contract.Requires(!string.IsNullOrWhiteSpace(sourceFile));
			Contract.Ensures(Contract.Result<Script>() != null);

			// Compile the script.
			var assembly = Compile(sourceFile, options);

			// Find the script class.
			var scriptType = assembly.GetType(ScriptClassName);
			if (scriptType == null || !scriptType.IsClass)
				throw new Exception(string.Format("Script must contain a class named '{0}'.", ScriptClassName));

			// Find the run method.
			var runMethod = scriptType.GetMethod(ScriptRunMethodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if (runMethod == null)
				throw new Exception(string.Format("Script must contain a static method '{0}.{1}'.", ScriptClassName, ScriptRunMethodName));

			if (runMethod.GetParameters().Any())
				throw new Exception(string.Format("'{0}.{1}' must take no parameters.", ScriptClassName, ScriptRunMethodName));

			// Return the Script object.
			return new Script(() => runMethod.Invoke(null, new object[] { }));
		}

		/// <summary>
		/// Compile the script contained in the given source file.
		/// </summary>
		/// <param name="sourceFile">The path to the script.</param>
		/// <param name="options">The script compiler options.  The default options will be used if this parameter is null.</param>
		/// <returns>A compiled assembly for the source file.</returns>
		public static Assembly Compile(string sourceFile, ScriptCompilerOptions options = null)
		{
			Contract.Requires(!string.IsNullOrWhiteSpace(sourceFile));
			Contract.Ensures(Contract.Result<Assembly>() != null);
			
			// Clean the options.
			options = options != null 
				? options.GetCleanOptions() 
				: new ScriptCompilerOptions();

			// Get all of the module references.
			var moduleReferences = GetModuleReferences(sourceFile);

			// Find all of the referenced module assemblies.  Anything that cannot be found in the module include paths
			// will be used directly.  Don't search for the module reference if it's already an absolute path.
			var referencedAssemblies = moduleReferences
				.Select(modRef => Path.IsPathRooted(modRef)
					? modRef
					: (options.ModuleIncludePaths.FirstOrDefault(path => File.Exists(Path.Combine(path, modRef))) ?? modRef))
				.Concat(options.ModuleReferences)
				.ToArray();

			// Create the script info file.
			var scriptInfoFilePath = Path.GetTempFileName();
			GenerateScriptInfoFile(scriptInfoFilePath, sourceFile);
			try
			{
				// Compile the file at the given path.
				var compilerParameters = new CompilerParameters(referencedAssemblies);

				// Make sure a PDB file is generated so we can debug.
				compilerParameters.IncludeDebugInformation = true;

				// We have to keep the temp files otherwise the pdb will be removed and we won't be able to debug.
				compilerParameters.TempFiles.KeepFiles = true;

				// Compile.
				var codeProvider = new CSharpCodeProvider();
				var compileResults = codeProvider.CompileAssemblyFromFile(compilerParameters, new string[] { sourceFile, scriptInfoFilePath });

				// Check if there were any errors or warnings in the script.
				if (compileResults.Errors.HasErrors)
					throw new ScriptCompilerException(compileResults.Errors);

				// Return the compiled and loaded assembly.
				return compileResults.CompiledAssembly;
			}
			finally
			{
				// Remove the script info file when we're done.
				File.Delete(scriptInfoFilePath);
			}
		}

		/// <summary>
		/// Generate the script info source file that gets compiled into each script.
		/// </summary>
		/// <param name="scriptInfoFilePath">The path where we should write the script info file.</param>
		/// <param name="scriptPath">The path to the script.</param>
		public static void GenerateScriptInfoFile(string scriptInfoFilePath, string scriptPath)
		{
			// Create the text of the script info file.
			var fileContents = string.Format(
@"
class ScriptInfo
{{
	public static readonly string Path = @""{0}"";
	public static readonly string Directory = @""{1}"";
}}
", scriptPath.Replace("\"", "\"\""), Path.GetDirectoryName(scriptPath).Replace("\"", "\"\""));

			File.WriteAllText(scriptInfoFilePath, fileContents);
		}

		/// <summary>
		/// Get the list of module references from the source file.
		/// </summary>
		/// <param name="sourceFiles">The source file.</param>
		/// <returns>An array of module references.</returns>
		public static string[] GetModuleReferences(string sourceFile)
		{
			var references = new List<string>();
			foreach (Match match in ModuleReferenceRegex.Matches(File.ReadAllText(sourceFile)))
			{
				references.Add(match.Groups["ref"].Value.Trim());
			}
			return references.ToArray();
		}

		/// <summary>
		/// The name of the script class within a script.
		/// </summary>
		private static string ScriptClassName = "Script";

		/// <summary>
		/// The name of the run method in the Script type.
		/// </summary>
		private static string ScriptRunMethodName = "Run";

		/// <summary>
		/// This regular expression is used to find module references in a script file.
		/// </summary>
		private static readonly Regex ModuleReferenceRegex = new Regex(@"^\s*//\s*require\s+(?<ref>.+)$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
	}
}
