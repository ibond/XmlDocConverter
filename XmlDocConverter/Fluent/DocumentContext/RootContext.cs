using NuDoq;
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
	public class RootContext : ScalarDocumentContext<RootContext>, IAssemblyContextProvider, IClassContextProvider
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
				return DocumentSource.AssemblyMembers.Select(source => new AssemblyContext(DocumentSource, source.Assembly));
			}
		}
		
		/// <summary>
		/// Groups the items in the EmitContext by class.
		/// </summary>
		public IEnumerable<ClassContext> Classes
		{
			get
			{
				return Assemblies.SelectMany(assembly => assembly.Classes);
			}
		}

		/// <summary>
		/// The default writer for the root context.
		/// </summary>
		public override EmitWriter<RootContext>.Writer DefaultWriter
		{
			get { return (context, doc) => context.Select.Assemblies().Write(); }
		}
	}
}
