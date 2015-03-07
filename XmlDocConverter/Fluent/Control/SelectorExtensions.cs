using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.Detail;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This class contains extension methods for moving between document contexts.
	/// </summary>
	public static class SelectorExtensions
	{
		/// <summary>
		/// Select all of the assemblies.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected assembly emit contexts.</returns>
		public static IEnumerable<EmitContext<AssemblyContext>> Assemblies(this IContextSelector<IAssemblyContextProvider> selector)
		{
			return selector.DocumentContext.Assemblies.Select(assemblyContext => selector.EmitContext.ReplaceDocumentContext(assemblyContext));
		}
	}
}
