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
	public class PropertyContext : MemberContext<PropertyContext, PropertyInfo>
	{
		public PropertyContext(DocumentSource documentSource, PropertyInfo info)
			:base(documentSource, info)
		{
		}

		/// <summary>
		/// The interface for an object that provides a property context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all properties.
			/// </summary>
			IEnumerable<PropertyContext> GetProperties(BindingFlags bindingFlags);
		}
	}


	/// <summary>
	/// The context selector extensions for PropertyContexts.
	/// </summary>
	public static class IPropertyContextProviderExtensions
	{
		/// <summary>
		/// Select all of the members.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected member emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<PropertyContext>, EmitContext<TDoc, TParent>>
			Properties<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, PropertyContext.IProvider> selector,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<PropertyContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentAndParentContext(
					new DocumentContextCollection<PropertyContext>(selector.DocumentContext.GetProperties(bindingFlags)),
					selector.EmitContext);
		}

		/// <summary>
		/// Select all of the classes from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<PropertyContext>, TParent>
			Members<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<PropertyContext.IProvider>> selector,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<PropertyContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentContext(
				new DocumentContextCollection<PropertyContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetProperties(bindingFlags))));
		}
	}
}
