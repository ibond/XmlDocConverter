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
		public static EmitContext<TDocContext, TParentContext> Scope<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context, Action<EmitContext<TDocContext, TParentContext>> scopeAction)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(scopeAction != null);
			Contract.Requires(Contract.Result<EmitContext<TDocContext, TParentContext>>() == context);

			// Call the scope action with this context.
			scopeAction(context);

			// Always return this context and ignore what happens in the scope action.
			return context;
		}


		/// <summary>
		/// Return control back to the parent emit context.  Useful for exiting out of a emit context collection without
		/// doing anything.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <returns>The parent emit context.</returns>
		public static TParentContext Break<TDocContext, TParentContext>(this EmitContext<TDocContext, TParentContext> context)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(Contract.Result<EmitContext<TDocContext, TParentContext>>() == context);
			
			return context.GetParentContext();
		}

		/// <summary>
		/// Executes the function for each EmitContext and returns to the collection parent.
		/// </summary>
		/// <param name="source">The current emit context.</param>
		/// <param name="action">The action containing the emit operation to run on each emit context.</param>
		/// <returns>The parent of the emit context.</returns>
		public static TParentContext
			ForEach<TDocContext, TParentContext>(
				this EmitContext<DocumentContextCollection<TDocContext>, TParentContext> source,
				Action<EmitContext<TDocContext, EmitContext<DocumentContextCollection<TDocContext>, TParentContext>>> action)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(source != null);
			Contract.Requires(action != null);
			Contract.Requires(Contract.Result<TParentContext>() == source.GetParentContext());
						
			// Run the action for each element.
			foreach (var element in source.GetDocumentContext().Elements)
			{
				source
					.ReplaceParentContext(source)
					.ReplaceDocumentContext(element)
					.Scope(action);
			}

			// Always return this context.
			return source.GetParentContext();
		}

		/// <summary>
		/// Executes the function for each EmitContext and returns to the collection parent.
		/// </summary>
		/// <param name="source">The current emit context.</param>
		/// <param name="action">The action containing the emit operation to run on each emit context.</param>
		/// <returns>The parent of the emit context.</returns>
		public static void ForEach<TDocContext>(
				this EmitContext<DocumentContextCollection<TDocContext>> source,
				Action<EmitContext<TDocContext>> action)
			where TDocContext : DocumentContext
		{
			Contract.Requires(source != null);
			Contract.Requires(action != null);

			// Run the action for each element.
			foreach (var element in source.GetDocumentContext().Elements)
			{
				source
					.ReplaceParentContext(source)
					.ReplaceDocumentContext(element)
					.Scope(action);
			}
		}
	}
}
