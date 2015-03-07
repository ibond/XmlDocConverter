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
		public static EmitContext<DocumentContextType, ParentEmitContextType> Scope<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, Action<EmitContext<DocumentContextType, ParentEmitContextType>> scopeAction)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(scopeAction != null);
			Contract.Requires(Contract.Result<EmitContext<DocumentContextType, ParentEmitContextType>>() == context);

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
		public static ParentEmitContextType Break<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);
			Contract.Requires(Contract.Result<EmitContext<DocumentContextType, ParentEmitContextType>>() == context);
			
			return context.GetParentContext();
		}

		/// <summary>
		/// Executes the function for each EmitContext and returns to the collection parent.
		/// </summary>
		/// <param name="source">The current emit context.</param>
		/// <param name="action">The action containing the emit operation to run on each emit context.</param>
		/// <returns>The parent of the emit context.</returns>
		public static ParentEmitContextType
			ForEach<DocumentContextType, ParentEmitContextType>(
				this EmitContext<DocumentContextCollection<DocumentContextType>, ParentEmitContextType> source,
				Action<EmitContext<DocumentContextType, EmitContext<DocumentContextCollection<DocumentContextType>, ParentEmitContextType>>> action)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(source != null);
			Contract.Requires(action != null);
			Contract.Requires(Contract.Result<ParentEmitContextType>() == source.GetParentContext());
						
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
	}
}
