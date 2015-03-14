using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// General extensions for flow control on the emit context.
	/// </summary>
	public static class ControlExtensions
	{
		/// <summary>
		/// Executes the emit operations in scopeAction then restores the EmitContext to it's original state.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="scopeAction">The action containing the scoped emit operations.</param>
		/// <returns>The EmitContext as it was before Scope was called.</returns>
		public static EmitContext<TDoc> Scope<TDoc>(this EmitContext<TDoc> context, Action<EmitContext<TDoc>> scopeAction)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(scopeAction != null);
			Contract.Requires(Contract.Result<EmitContext<TDoc>>() == context);

			// Call the scope action with this context.
			scopeAction(context);

			// Always return this context and ignore what happens in the scope action.
			return context;
		}


		/// <summary>
		/// Executes the function for each EmitContext and returns to the collection parent.
		/// </summary>
		/// <param name="source">The current emit context.</param>
		/// <param name="action">The action containing the emit operation to run on each emit context.</param>
		/// <returns>The parent of the emit context.</returns>
		public static EmitContext<DocumentContextCollection<TDoc>>
			ForEach<TDoc>(
				this EmitContext<DocumentContextCollection<TDoc>> source,
				Action<EmitContext<TDoc>> action)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(source != null);
			Contract.Requires(action != null);
						
			// Run the action for each element.
			foreach (var element in source.GetDocumentContext().Elements)
			{
				source
					.ReplaceDocumentContext(element)
					.Scope(action);
			}

			// Always return this context.
			return source;
		}
	}
}
