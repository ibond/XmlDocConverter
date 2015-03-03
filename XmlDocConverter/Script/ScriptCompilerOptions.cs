using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// This class contains the options that will be used to compile scripts.
	/// </summary>
	public class ScriptCompilerOptions
	{
		/// <summary>
		/// Construct a default empty set of options.
		/// </summary>
		public ScriptCompilerOptions()
		{
			ModuleReferences = new List<string>();
			ModuleIncludePaths = new List<string>();
		}

		/// <summary>
		/// Get a new set of compiler options with invalid values (e.g. null) replaced.
		/// </summary>
		/// <returns>A new ScriptCompilerOptions object with cleaned up values.</returns>
		public ScriptCompilerOptions GetCleanOptions()
		{
			var newOptions = new ScriptCompilerOptions();
			newOptions.ModuleReferences.AddRange(this.ModuleReferences);
			newOptions.ModuleIncludePaths.AddRange(this.ModuleIncludePaths);

			return newOptions;
		}

		/// <summary>
		/// The list of forced module references.  The ModuleIncludePaths will not be used to find these references.
		/// </summary>
		public List<string> ModuleReferences { get; set; }

		/// <summary>
		/// The include paths to be searched when looking for modules.
		/// </summary>
		public List<string> ModuleIncludePaths { get; set; }
	}
}
