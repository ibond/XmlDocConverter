using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter
{
	/// <summary>
	/// This class is used to start the fluent emitter interface.
	/// </summary>
	public static class Emit
	{
		/// <summary>
		/// This begins the emit process using the initial emit context specified by the script context.
		/// </summary>
		public static EmitContext<RootContext> Begin
		{
			get
			{
				return EmitContext.CreateRoot(Script.CurrentRunContext.InitialEmitContext.ClonePersistentData());
			}
		}

		/// <summary>
		/// This begins the emit process using a clean emit context.
		/// </summary>
		public static EmitContext<RootContext> BeginClean
		{
			get
			{
				return EmitContext.CreateRoot();
			}
		}
	}
}
