using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Detail;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A context for an assembly.
	/// </summary>
	public class AssemblyContext : DocumentContext, IClassContextProvider
	{
		/// <summary>
		/// Construct an AssemblyContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		/// <param name="assembly">The Assembly contained within this item.</param>
		public AssemblyContext(DocumentSource documentSource, Assembly assembly)
			: base(documentSource)
		{
			Contract.Requires(assembly != null);
			Contract.Ensures(this.m_assembly != null);

			m_assembly = assembly;
		}

		/// <summary>
		/// Gets the name of this assembly.
		/// </summary>
		public string Name { get { return m_assembly.GetName().Name; } }

		/// <summary>
		/// Gets the assembly for this item.
		/// </summary>
		public Assembly Assembly { get { return m_assembly; } }

		/// <summary>
		/// Get the classes contained within this assembly.
		/// </summary>
		public IEnumerable<ClassContext> Classes
		{
			get
			{
				return m_assembly.GetTypes()
					.Where(type => type.IsClass)
					.Select(type => new ClassContext(DocumentSource, type));
			}
		}

		/// <summary>
		/// The Assembly for this AssemblyItem.
		/// </summary>
		private readonly Assembly m_assembly;
	}


	/// <summary>
	/// The interface for an object that provides an assembly context.
	/// </summary>
	public interface IAssemblyContextProvider
	{
		/// <summary>
		/// Get all assemblies.
		/// </summary>
		IEnumerable<AssemblyContext> Assemblies { get; }
	}

	
	/// <summary>
	/// This context selector extensions for IAssemblyContextProvider.
	/// </summary>
	public static class IAssemblyContextProviderExtensions
	{
		/// <summary>
		/// Select all of the assemblies.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected assembly emit contexts.</returns>
		public static EmitContextCollection<AssemblyContext, SourceDocumentContextType> Assemblies<SourceDocumentContextType>(this IContextSelector<SourceDocumentContextType, IAssemblyContextProvider> selector)
			where SourceDocumentContextType : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContextCollection<AssemblyContext, SourceDocumentContextType>>() != null);

			return new EmitContextCollection<AssemblyContext, SourceDocumentContextType>(
				selector.DocumentContext.Assemblies.Select(assemblyContext => selector.EmitContext.ReplaceDocumentContext(assemblyContext)), selector.EmitContext);
		}
	}
}
