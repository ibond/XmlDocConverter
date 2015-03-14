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
	/// A render filter takes a block of rendered output and filters or converts it to another format.
	/// </summary>
	public interface IRenderFilter
	{
		/// <summary>
		/// Apply the render filter to the given source data and return the result.
		/// </summary>
		/// <param name="source">The incoming source data.</param>
		/// <returns>The output data after the filter has been applied.</returns>
		IEnumerable<string> Apply(IEnumerable<string> source);
	}

	public static class RenderFilterExtensions
	{
		public static EmitContext<TDoc> WithFilter<TDoc>(this EmitContext<TDoc> context, IRenderFilter filter, Action<EmitContext<TDoc>> action)
			where TDoc : DocumentContext
		{
			Contract.Requires(context != null);
			Contract.Requires(filter != null);
			
			// Create a new output context and render filter source.
			var outputContext = new EmitOutputContext();
			var renderFilterSource = new RenderFilterOutputSource(outputContext, filter);

			// Write the filter source to the current context.
			context.GetOutputContext().Write(renderFilterSource);

			// Call the action with the new output context.
			action(context.ReplaceOutputContext(outputContext));

			// Return the original context.
			return context;
		}

		class RenderFilterOutputSource : IOutputSource
		{
			public RenderFilterOutputSource(IOutputSource source, IRenderFilter filter)
			{
				m_source = source;
				m_filter = filter;
			}

			public IEnumerable<string> Data
			{
				get { return m_filter.Apply(m_source.Data); }
			}

			private readonly IOutputSource m_source;
			private readonly IRenderFilter m_filter;
		}

		public static IRenderFilter Then(this IRenderFilter firstFilter, IRenderFilter secondFilter)
		{
			return new RenderFilter(data => secondFilter.Apply(firstFilter.Apply(data)));
		}
	}
}
