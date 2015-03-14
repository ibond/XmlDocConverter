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
		/// Select all of the properties.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Properties<TDoc>(
				this IContextSelector<TDoc, PropertyContext.IProvider> selector,
				Action<EmitContext<DocumentContextCollection<PropertyContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<PropertyContext>>>() != null);

			action(selector.EmitContext
				.ReplaceDocumentContext(new DocumentContextCollection<PropertyContext>(selector.DocumentContext.GetProperties(bindingFlags))));

			return selector.EmitContext;
		}

		/// <summary>
		/// Select all of the properties from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Properties<TDoc>(
				this IContextSelector<TDoc, IDocumentContextCollection<PropertyContext.IProvider>> selector,
				Action<EmitContext<DocumentContextCollection<PropertyContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<PropertyContext>>>() != null);

			var col = new DocumentContextCollection<PropertyContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetProperties(bindingFlags)));

			action(selector.EmitContext.ReplaceDocumentContext(col));

			return selector.EmitContext;
		}
	}
}
