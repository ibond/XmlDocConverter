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
		public static IEnumerable<EmitContext<AssemblyContext>> Assemblies<DocumentContextType>(this IContextSelector<DocumentContextType, IAssemblyContextProvider> selector)
			where DocumentContextType : DocumentContext
		{
			return selector.DocumentContext.Assemblies.Select(assemblyContext => selector.EmitContext.ReplaceDocumentContext(assemblyContext));
		}
	}
}
