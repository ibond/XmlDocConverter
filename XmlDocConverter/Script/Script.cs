using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter
{
	/// <summary>
	/// This is the delegate type for running a script.
	/// </summary>
	public delegate void RunScriptDelegate();

	/// <summary>
	/// This class represents a loaded script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// Construct a script object.
		/// </summary>
		/// <param name="runScript">The delegate used to run this script.</param>
		public Script(RunScriptDelegate runScript)
		{
			Contract.Requires(runScript != null);

			m_runScript = runScript;
		}

		/// <summary>
		/// Run the script.
		/// </summary>
		public void Run()
		{
			m_runScript();
		}

		/// <summary>
		/// The delegate for running this script.
		/// </summary>
		private RunScriptDelegate m_runScript;
	}
}
