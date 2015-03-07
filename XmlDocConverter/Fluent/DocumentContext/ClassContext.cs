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
	/// A context for a class.
	/// </summary>
	public class ClassContext : DocumentContext
	{
		/// <summary>
		/// Construct an ClassContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		/// <param name="classType">The Type for the class contained within this context.</param>
		public ClassContext(DocumentSource documentSource, Type classType)
			: base(documentSource)
		{
			Contract.Requires(classType != null);
			Contract.Requires(classType.IsClass);
			Contract.Ensures(this.m_classType != null);

			m_classType = classType;
		}

		/// <summary>
		/// Gets the name of this class.
		/// </summary>
		public string Name { get { return m_classType.Name; } }

		/// <summary>
		/// Gets the class type for this item.
		/// </summary>
		public Type Class { get { return m_classType; } }

		/// <summary>
		/// The Type for this AssemblyItem.
		/// </summary>
		private readonly Type m_classType;
	}


	/// <summary>
	/// The interface for an object that provides an class context.
	/// </summary>
	public interface IClassContextProvider
	{
		/// <summary>
		/// Get all classes.
		/// </summary>
		IEnumerable<ClassContext> Classes { get; }
	}


	/// <summary>
	/// This context selector extensions for IClassContextProvider.
	/// </summary>
	public static class IClassContextProviderExtensions
	{
		/// <summary>
		/// Select all of the classes.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContextCollection<ClassContext, SourceDocumentContextType> Classes<SourceDocumentContextType>(this IContextSelector<SourceDocumentContextType, IClassContextProvider> selector)
			where SourceDocumentContextType : DocumentContext
		{
			return new EmitContextCollection<ClassContext, SourceDocumentContextType>(
				selector.DocumentContext.Classes.Select(ClassContext => selector.EmitContext.ReplaceDocumentContext(ClassContext)), selector.EmitContext);
		}
	}
}
