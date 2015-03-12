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
		/// Select all of the members.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected member emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<FieldContext>, EmitContext<TDoc, TParent>>
			Fields<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, FieldContext.IProvider> selector,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<FieldContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentAndParentContext(
					new DocumentContextCollection<FieldContext>(selector.DocumentContext.GetFields(bindingFlags)),
					selector.EmitContext);
		}

		/// <summary>
		/// Select all of the classes from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<FieldContext>, TParent>
			Members<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<FieldContext.IProvider>> selector,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<FieldContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentContext(
				new DocumentContextCollection<FieldContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetFields(bindingFlags))));
		}
	}
}
