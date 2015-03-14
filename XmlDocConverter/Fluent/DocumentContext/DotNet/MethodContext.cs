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
	public class MethodContext : MemberContext<MethodContext, MethodInfo>
	{
		public MethodContext(DocumentSource documentSource, MethodInfo info)
			: base(documentSource, info)
		{
		}

		/// <summary>
		/// The interface for an object that provides a method context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all methods.
			/// </summary>
			IEnumerable<MethodContext> GetMethods(BindingFlags bindingFlags);
		}
	}


	/// <summary>
	/// The context selector extensions for MethodContexts.
	/// </summary>
	public static class IMethodContextProviderExtensions
	{
		/// <summary>
		/// Select all of the methods.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Methods<TDoc>(
				this IContextSelector<TDoc, MethodContext.IProvider> selector,
				Action<EmitContext<DocumentContextCollection<MethodContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<MethodContext>>>() != null);

			action(selector.EmitContext
				.ReplaceDocumentContext(new DocumentContextCollection<MethodContext>(selector.DocumentContext.GetMethods(bindingFlags))));

			return selector.EmitContext;
		}

		/// <summary>
		/// Select all of the methods from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected struct emit contexts.</returns>
		public static EmitContext<TDoc>
			Methods<TDoc>(
				this IContextSelector<TDoc, IDocumentContextCollection<MethodContext.IProvider>> selector,
				Action<EmitContext<DocumentContextCollection<MethodContext>>> action,
				BindingFlags bindingFlags = MemberContext.DefaultBindingFlags)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<MethodContext>>>() != null);

			var col = new DocumentContextCollection<MethodContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetMethods(bindingFlags)));

			action(selector.EmitContext.ReplaceDocumentContext(col));

			return selector.EmitContext;
		}
	}
}
