using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public static EmitContext<DocumentContextType> Scope<DocumentContextType>(this EmitContext<DocumentContextType> context, Action<EmitContext<DocumentContextType>> scopeAction)
			where DocumentContextType : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(scopeAction != null);
			Contract.Requires(Contract.Result<EmitContext<DocumentContextType>>() == context);

			// Call the scope action with this context.
			scopeAction(context);

			// Always return this context and ignore what happens in the scope action.
			return context;
		}

		/// <summary>
		/// Executes the function for each EmitContext and returns to the collection source.
		/// </summary>
		/// <param name="source">The current emit context.</param>
		/// <param name="action">The action containing the emit operation to run on each emit context.</param>
		/// <returns>The the source of the EmitContextCollection.</returns>
		public static EmitContext<SourceDocumentContextType> 
			ForEach<DocumentContextType, SourceDocumentContextType>(
				this EmitContextCollection<DocumentContextType, SourceDocumentContextType> source, 
				Action<EmitContext<DocumentContextType>> action)
			where DocumentContextType : DocumentContext
			where SourceDocumentContextType : DocumentContext
		{
			Contract.Requires(source != null);
			Contract.Requires(action != null);
			Contract.Requires(Contract.Result<EmitContextCollection<DocumentContextType, SourceDocumentContextType>>() == source);

			// Run the action for each element.
			foreach (var element in source.Contexts)
			{
				action(element);
			}

			// Always return this context.
			return source.Source;
		}
	}
}
