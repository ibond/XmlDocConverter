using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Detail;
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
		protected override EmitWriter<AssemblyContext>.Writer GetDefaultRenderer()
		{
			return item => item.Emit.Select.Classes(classes => classes.Render());
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
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Assemblies<TDoc>(
				this IContextSelector<TDoc, AssemblyContext.IProvider> selector,
				Action<EmitContext<DocumentContextCollection<AssemblyContext>>> action)
			where TDoc : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>>>() != null);

			action(selector.EmitContext
				.ReplaceDocumentContext(new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.GetAssemblies())));

			return selector.EmitContext;
		}

		/// <summary>
		/// Select all of the assemblies from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Assemblies<TDoc>(
				this IContextSelector<TDoc, IDocumentContextCollection<AssemblyContext.IProvider>> selector,
				Action<EmitContext<DocumentContextCollection<AssemblyContext>>> action)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<AssemblyContext>>>() != null);

			var col = new DocumentContextCollection<AssemblyContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetAssemblies()));

			action(selector.EmitContext.ReplaceDocumentContext(col));

			return selector.EmitContext;
		}
	}
}
