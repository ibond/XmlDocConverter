using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Detail;
using RazorEngine.Templating;
using System.Runtime.CompilerServices;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A context for an assembly.
	/// </summary>
	public class AssemblyContext : DotNetDocumentContext<AssemblyContext>, ClassContext.IProvider, StructContext.IProvider
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
		/// The default writer for an assembly.
		/// </summary>
		protected override Action<EmitWriterItem<AssemblyContext>> GetDefaultWriter()
		{
			return item => item.Emit.Select.Classes().Write();
		}

		/// <summary>
		/// Get the classes contained within this assembly.
		/// </summary>
		public IEnumerable<ClassContext> GetClasses()
		{
			return m_assembly.GetTypes()
				.Where(type => type.IsClass && Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) == null)
				.Select(type => new ClassContext(DocumentSource, type));
		}

		public IEnumerable<StructContext> GetStructs()
		{
			return m_assembly.GetTypes()
				.Where(type => 
					type.IsValueType 
					&& !type.IsEnum 
					&& !type.IsPrimitive 
					&& Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) == null)
				.Select(type => new StructContext(DocumentSource, type));
		}
		
		/// <summary>
		/// The Assembly for this AssemblyItem.
		/// </summary>
		private readonly Assembly m_assembly;


		/// <summary>
		/// The interface for an object that provides an assembly context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all assemblies.
			/// </summary>
			IEnumerable<AssemblyContext> GetAssemblies();
		}
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
		public static EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<TDoc, TParent>> 
			Assemblies<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, AssemblyContext.IProvider> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext
				.ReplaceParentContext(selector.EmitContext)
				.ReplaceDocumentContext(new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.GetAssemblies()));
		}

		/// <summary>
		/// Select all of the assemblies from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected assembly emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<AssemblyContext>, TParent> 
			Assemblies<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<AssemblyContext.IProvider>> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>, EmitContext<TDoc, TParent>>>() != null);

			var col = new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetAssemblies()));
			return selector.EmitContext
				.ReplaceDocumentContext(col);
		}
	}

	///// <summary>
	///// This provides the extensions for writing AssemblyContext objects..
	///// </summary>
	//public static class AssemblyContextWriterExtensions
	//{
	//	public static TParent Write<TParent>(this EmitContext<DocumentContextCollection<RootContext>, TParent> context)
	//		where TParent : EmitContext
	//	{
	//		return context.ForEach(emit => emit.Write());
	//	}

	//	public static EmitContext<RootContext, TParent> Write<TParent>(this EmitContext<RootContext, TParent> context)
	//		where TParent : EmitContext
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
	//	public static EmitContext<TDoc, TParent> InDirectory<TDoc, TParent>(this EmitContext<TDoc, TParent> context, string directoryPath)
	//		where TDoc : DocumentContext
	//		where TParent : EmitContext
	//	{
	//		Contract.Requires(context != null);
	//		Contract.Requires(!String.IsNullOrWhiteSpace(directoryPath));
	//		Contract.Requires(Contract.Result<EmitContext<TDoc, TParent>>() != null);

	//		// Set the base directory.
	//		return context.ReplaceLocalDataMap(context.GetLocalDataMap().SetItem(BaseDirectoryDataMapKey, directoryPath));
	//	}

	//	/// <summary>
	//	/// This object is used as a key into the emit context data map for storing the assembly context write function.
	//	/// </summary>
	//	private static object AssemblyContextWriterKey = new object();
	//}
}
