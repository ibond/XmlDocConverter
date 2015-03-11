using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// The base document context type.
	/// </summary>
	public abstract class DocumentContext
	{
	}


	/// <summary>
	/// The typed document context type.
	/// </summary>
	/// <typeparam name="TDerived">The type of the actual instance of this document context.</typeparam>
	public abstract class DocumentContext<TDerived> : DocumentContext
		where TDerived : DocumentContext<TDerived>
	{
		/// <summary>
		/// Get the default writer for this document context.
		/// </summary>
		protected virtual Action<EmitWriterItem<TDerived>> GetDefaultWriter()
		{
			return item => { };
		}

		/// <summary>
		/// Get the default writer wrapped with a TemplateWriter.
		/// </summary>
		public EmitWriter<TDerived>.Writer DefaultWriter
		{
			get
			{
				return item => new TemplateWriter(dummyWriter => GetDefaultWriter()(item));
			}
		}
	}
}
