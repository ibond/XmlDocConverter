using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
			using (var runContext = new ScriptRunContext())
			{
				// Set the call context for this so it's available to all functions in the script.
				CallContext.LogicalSetData(ScriptRunContextDataName, runContext);
				try
				{
					m_runScript();
				}
				finally
				{
					// Clear the call context when we're done running.
					CallContext.FreeNamedDataSlot(ScriptRunContextDataName);
				}
			}
		}

		/// <summary>
		/// Get the current script run context.
		/// </summary>
		public static ScriptRunContext CurrentRunContext
		{
			get
			{
				return (ScriptRunContext)CallContext.LogicalGetData(ScriptRunContextDataName);
			}
		}

		/// <summary>
		/// The name of the data in the logical call context.
		/// </summary>
		private static readonly string ScriptRunContextDataName = "XmlDocConverter.ScriptRunContext";

		/// <summary>
		/// The delegate for running this script.
		/// </summary>
		private RunScriptDelegate m_runScript;
	}
}
