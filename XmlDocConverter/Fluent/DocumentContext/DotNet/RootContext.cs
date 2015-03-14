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
	public class RootContext : DotNetDocumentContext<RootContext>, AssemblyContext.IProvider, ClassContext.IProvider
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
		public IEnumerable<AssemblyContext> GetAssemblies()
		{
			return DocumentSource.Assemblies.Select(assembly => new AssemblyContext(DocumentSource, assembly));		
		}
		
		/// <summary>
		/// Groups the items in the EmitContext by class.
		/// </summary>
		public IEnumerable<ClassContext> GetClasses()
		{			
			return GetAssemblies().SelectMany(assembly => assembly.GetClasses());
		}

		/// <summary>
		/// The default writer for the root context.
		/// </summary>
		protected override EmitWriter<RootContext>.Writer GetDefaultRenderer()
		{
			return item => item.Emit.Select.Assemblies(assemblies => assemblies.Render());
		}
	}
}
