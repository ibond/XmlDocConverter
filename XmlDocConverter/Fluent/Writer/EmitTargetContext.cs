using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is the delegate for writing data to a target. e.g. a file, stream, or string builder.
	/// </summary>
	/// <param name="dataSources">The data source to be written to this target.  This contains all of the data that will be written.</param>
	public delegate void WriteDataToTargetDelegate(IEnumerable<string> dataSources);

	/// <summary>
	/// This is the context for an emit target.
	/// </summary>
	public class EmitTargetContext
	{
		/// <summary>
		/// Construct a new target context.
		/// </summary>
		/// <param name="outputContext">The writer context for this target.</param>
		/// <param name="target">The function to be called when we should write the data.</param>
		public EmitTargetContext(EmitOutputContext outputContext, WriteDataToTargetDelegate target)
		{
			Contract.Requires(outputContext != null);
			Contract.Requires(target != null);
			Contract.Ensures(OutputContext != null);
			Contract.Ensures(Target != null);

			OutputContext = outputContext;
			Target = target;
		}

		/// <summary>
		/// The writer context for this target.
		/// </summary>
		public readonly EmitOutputContext OutputContext;

		/// <summary>
		/// The target to which we should write data when we are done.
		/// </summary>
		public readonly WriteDataToTargetDelegate Target;
	}
}
