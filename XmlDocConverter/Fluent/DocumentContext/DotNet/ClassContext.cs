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

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A context for a class.
	/// </summary>
	public class ClassContext : EncapsulatingTypeContext<ClassContext>
	{
		/// <summary>
		/// Construct an ClassContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		/// <param name="classType">The Type for the class contained within this context.</param>
		public ClassContext(DocumentSource documentSource, Type classType)
			: base(documentSource, classType)
		{
			Contract.Requires(classType.IsClass);
		}

		/// <summary>
		/// The interface for an object that provides a class context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all properties.
			/// </summary>
			IEnumerable<ClassContext> GetClasses();
		}
	}


	/// <summary>
	/// This context selector extensions for ClassContext.IProvider.
	/// </summary>
	public static class IClassContextProviderExtensions
	{
		/// <summary>
		/// Select all of the classes.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<ClassContext>, EmitContext<TDoc, TParent>>
			Classes<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, ClassContext.IProvider> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<ClassContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext
				.ReplaceParentContext(selector.EmitContext)
				.ReplaceDocumentContext(new DocumentContextCollection<ClassContext>(selector.DocumentContext.GetClasses()));
		}

		/// <summary>
		/// Select all of the classes from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<ClassContext>, TParent>
			Classes<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<ClassContext.IProvider>> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<ClassContext>, EmitContext<TDoc, TParent>>>() != null);

			var col = new DocumentContextCollection<ClassContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetClasses()));
			return selector.EmitContext
				.ReplaceDocumentContext(col);
		}
	}
}
