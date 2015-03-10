using NuDoq;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.Detail;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is a context representing a NuDoq Container which contains documentation data.
	/// </summary>
	public class NuDoqContainerContext : DotNetDocumentContext<NuDoqContainerContext>
	{
		/// <summary>
		/// Construct an NuDoqContainerContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public NuDoqContainerContext(DocumentSource documentSource, Container container)
			: base(documentSource)
		{
			m_container = container ?? new EmptyContainer();
		}

		protected override Action<EmitContext<NuDoqContainerContext>> GetDefaultWriter()
		{
			return emit =>
				{
					var text = m_container.ToText();
					emit.GetOutputContext().Write(text);
				};
		}

		/// <summary>
		/// The document container.
		/// </summary>
		private readonly Container m_container;

		/// <summary>
		/// The interface for getting a summary container.
		/// </summary>
		public interface ISummaryProvider
		{
			/// <summary>
			/// Get the summary container.
			/// </summary>
			NuDoqContainerContext GetSummary();
		}
	}
	
	/// <summary>
	/// Selector extensions for NuDoqContainerContext.
	/// </summary>
	public static class NuDoqContainerContextProviderExtensions
	{
		/// <summary>
		/// Select all of the classes.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<NuDoqContainerContext, EmitContext<TDoc, TParent>>

			Summary<TDoc, TParent>(this IContextSelector<TDoc, TParent, NuDoqContainerContext.ISummaryProvider> selector)

			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<NuDoqContainerContext, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentAndParentContext(
				selector.DocumentContext.GetSummary(),
				selector.EmitContext);
		}
	}
}
