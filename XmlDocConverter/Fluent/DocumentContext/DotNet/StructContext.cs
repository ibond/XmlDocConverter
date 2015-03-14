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
	/// A context for a struct.
	/// </summary>
	public class StructContext : EncapsulatingTypeContext<StructContext>
	{
		/// <summary>
		/// Construct an StructContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		/// <param name="structType">The Type for the struct contained within this context.</param>
		public StructContext(DocumentSource documentSource, Type structType)
			: base(documentSource, structType)
		{
			Contract.Requires(structType.IsValueType && !structType.IsEnum && !structType.IsPrimitive);
		}

		/// <summary>
		/// The interface for an object that provides a struct context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all properties.
			/// </summary>
			IEnumerable<StructContext> GetStructs();
		}
	}


	/// <summary>
	/// This context selector extensions for StructContext.IProvider.
	/// </summary>
	public static class IStructContextProviderExtensions
	{
		/// <summary>
		/// Select all of the structes.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<StructContext>, EmitContext<TDoc, TParent>>
			Structs<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, StructContext.IProvider> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<StructContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext
				.ReplaceParentContext(selector.EmitContext)
				.ReplaceDocumentContext(new DocumentContextCollection<StructContext>(selector.DocumentContext.GetStructs()));
		}

		/// <summary>
		/// Select all of the structes from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<StructContext>, TParent>
			Structs<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<StructContext.IProvider>> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<StructContext>, EmitContext<TDoc, TParent>>>() != null);

			var col = new DocumentContextCollection<StructContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetStructs()));
			return selector.EmitContext
				.ReplaceDocumentContext(col);
		}
	}
}
