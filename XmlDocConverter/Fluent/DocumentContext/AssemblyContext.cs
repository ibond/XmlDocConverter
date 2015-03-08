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
	public class AssemblyContext : ScalarDocumentContext<AssemblyContext>, IClassContextProvider
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

		public override Func<EmitContext<AssemblyContext>, EmitContext> DefaultWriter
		{
			get { return context => context; }
		}

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
		public static EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<SourceDocumentContextType, SourceParentEmitContextType>> Assemblies<SourceDocumentContextType, SourceParentEmitContextType>(this IContextSelector<SourceDocumentContextType, SourceParentEmitContextType, IAssemblyContextProvider> selector)
			where SourceDocumentContextType : DocumentContext
			where SourceParentEmitContextType : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<SourceDocumentContextType, SourceParentEmitContextType>>>() != null);

			return selector.EmitContext
				.ReplaceParentContext(selector.EmitContext)
				.ReplaceDocumentContext(new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.Assemblies));
		}

		/// <summary>
		/// Select all of the assemblies from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected assembly emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<AssemblyContext>, SourceParentEmitContextType> 
			Assemblies<SourceDocumentContextType, SourceParentEmitContextType>(
				this IContextSelector<SourceDocumentContextType, SourceParentEmitContextType, IDocumentContextCollection<IAssemblyContextProvider>> selector)
			where SourceDocumentContextType : DocumentContext
			where SourceParentEmitContextType : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<SourceDocumentContextType, SourceParentEmitContextType>>>() != null);

			var col = new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.Elements.SelectMany(element => element.Assemblies));
			return selector.EmitContext
				.ReplaceDocumentContext(col);
		}
	}

	///// <summary>
	///// This provides the extensions for writing AssemblyContext objects..
	///// </summary>
	//public static class AssemblyContextWriterExtensions
	//{
	//	public static TParentContext Write<TParentContext>(this EmitContext<DocumentContextCollection<RootContext>, TParentContext> context)
	//		where TParentContext : EmitContext
	//	{
	//		return context.ForEach(emit => emit.Write());
	//	}

	//	public static EmitContext<RootContext, TParentContext> Write<TParentContext>(this EmitContext<RootContext, TParentContext> context)
	//		where TParentContext : EmitContext
	//	{
	//		return context;
	//		//return context
	//		//	.Select.Assemblies()
	//		//	.Write();
	//	}

	//	/// <summary>
	//	/// Set the base directory for where we are emitting files.
	//	/// </summary>
	//	/// <param name="directoryPath">The path to the base directory.</param>
	//	/// <returns>A new context with the updated base directory.</returns>
	//	public static EmitContext<TDocContext, TParentContext> InDirectory<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, string directoryPath)
	//		where TDocContext : DocumentContext
	//		where TParentContext : EmitContext
	//	{
	//		Contract.Requires(context != null);
	//		Contract.Requires(!String.IsNullOrWhiteSpace(directoryPath));
	//		Contract.Requires(Contract.Result<EmitContext<TDocContext, TParentContext>>() != null);

	//		// Set the base directory.
	//		return context.ReplaceLocalDataMap(context.GetLocalDataMap().SetItem(BaseDirectoryDataMapKey, directoryPath));
	//	}

	//	/// <summary>
	//	/// This object is used as a key into the emit context data map for storing the assembly context write function.
	//	/// </summary>
	//	private static object AssemblyContextWriterKey = new object();
	//}
}
