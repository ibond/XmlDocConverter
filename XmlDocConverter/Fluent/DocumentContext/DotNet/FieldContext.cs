using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.Detail;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public class FieldContext : MemberContext<FieldContext, FieldInfo>
	{
		public FieldContext(DocumentSource documentSource, FieldInfo info)
			: base(documentSource, info)
		{
		}

		/// <summary>
		/// The interface for an object that provides a field context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all fields.
			/// </summary>
			IEnumerable<FieldContext> GetFields(BindingFlags bindingFlags);
		}
	}


	/// <summary>
	/// The context selector extensions for FieldContexts.
	/// </summary>
	public static class IFieldContextProviderExtensions
	{
		/// <summary>
		/// Select all of the fields.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Fields<TDoc>(
				this IContextSelector<TDoc, FieldContext.IProvider> selector,
				Action<EmitContext<DocumentContextCollection<FieldContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<FieldContext>>>() != null);

			action(selector.EmitContext
				.ReplaceDocumentContext(new DocumentContextCollection<FieldContext>(selector.DocumentContext.GetFields(bindingFlags))));

			return selector.EmitContext;
		}

		/// <summary>
		/// Select all of the fields from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Fields<TDoc>(
				this IContextSelector<TDoc, IDocumentContextCollection<FieldContext.IProvider>> selector,
				Action<EmitContext<DocumentContextCollection<FieldContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<FieldContext>>>() != null);

			var col = new DocumentContextCollection<FieldContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetFields(bindingFlags)));

			action(selector.EmitContext.ReplaceDocumentContext(col));

			return selector.EmitContext;
		}
	}
}
