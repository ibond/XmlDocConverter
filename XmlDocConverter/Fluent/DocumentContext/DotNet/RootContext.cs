using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// The root context for emitting documents.  This effectively contains a list of assemblies.
	/// </summary>
	public class RootContext : DotNetDocumentContext<RootContext>, IAssemblyContextProvider, ClassContext.IProvider
	{
		/// <summary>
		/// Construct an RootContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public RootContext(DocumentSource documentSource)
			: base(documentSource)
		{
		}

		/// <summary>
		/// Groups the items in the EmitContext by Assembly.
		/// </summary>
		public IEnumerable<AssemblyContext> Assemblies
		{
			get
			{
				return DocumentSource.Assemblies.Select(assembly => new AssemblyContext(DocumentSource, assembly));
			}
		}
		
		/// <summary>
		/// Groups the items in the EmitContext by class.
		/// </summary>
		public IEnumerable<ClassContext> GetClasses()
		{
			return Assemblies.SelectMany(assembly => assembly.GetClasses());
		}

		/// <summary>
		/// The default writer for the root context.
		/// </summary>
		protected override Action<EmitWriterItem<RootContext>> GetDefaultWriter()
		{
			return item => item.Emit.Select.Assemblies().Write();
		}
	}
}
