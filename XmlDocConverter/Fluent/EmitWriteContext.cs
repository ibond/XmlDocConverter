using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	#region EmitWriteContext<TDoc>
	// =====================================================================

	/// <summary>
	/// This derived emit context is used to chain Write commands without requiring repeated Write prefixes.
	/// </summary>
	public class EmitWriteContext<TDoc> : EmitContext<TDoc>
		where TDoc : DocumentContext
	{
		#region Constructors and Conversions
		// =====================================================================

		internal EmitWriteContext(
			TDoc documentContext,
			ConcurrentDictionary<object, object> persistentDataMap,
			ImmutableDictionary<object, object> localDataMap,
			EmitOutputContext outputContext)
			: base(documentContext, persistentDataMap, localDataMap, outputContext)
		{
		}

		public static implicit operator EmitContextX(EmitWriteContext<TDoc> context)
		{
			return new EmitContextX(context.m_persistentDataMap, context.m_localDataMap, context.m_outputContext);
		}

		// =====================================================================
		#endregion
	}

	// =====================================================================
	#endregion


	#region EmitWriteContext<TDoc> Extensions
	// =====================================================================

	/// <summary>
	/// Extension methods to make using writers more fluent.
	/// </summary>
	public static class WriterExtensions
	{
		public static EmitWriteContext<TDoc> L<TDoc>(this EmitWriteContext<TDoc> context)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().WriteLine();
			return context;
		}

		public static EmitWriteContext<TDoc> L<TDoc>(this EmitWriteContext<TDoc> context, string value)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().WriteLine(value);
			return context;
		}

		public static EmitWriteContext<TDoc> L<TDoc>(this EmitWriteContext<TDoc> context, string value, params object[] args)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().WriteLine(string.Format(value, args));
			return context;
		}


		public static EmitWriteContext<TDoc> A<TDoc>(this EmitWriteContext<TDoc> context, string value)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().Write(value);
			return context;
		}

		public static EmitWriteContext<TDoc> A<TDoc>(this EmitWriteContext<TDoc> context, string value, params object[] args)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().Write(string.Format(value, args));
			return context;
		}


		public static EmitWriteContext<TDoc> Source<TDoc>(this EmitWriteContext<TDoc> context, IOutputSource source)
			where TDoc : DocumentContext
		{
			context.GetOutputContext().Write(source);
			return context;
		}


		public static EmitWriteContext<TDoc> Link<TDoc>(this EmitWriteContext<TDoc> context, string targetKey, string contents)
			where TDoc : DocumentContext
		{
			return context
				.WithFilter(
					new RenderFilter((string data) =>
					{
						string targetRef;
						if(context.GetPersistentDataSubmap(LinkTargets).TryGetValue(targetKey, out targetRef))
							return String.Format("[{0}]({1})", data, targetRef);
						else
							return data;
					}),
					emit => emit.Write.A(contents))
				.Write;
		}


		public static EmitContext<TDoc> SetLinkTarget<TDoc>(this EmitContext<TDoc> context, string targetKey, string targetRef)
			where TDoc : DocumentContext
		{
			var resultTarget = context.GetPersistentDataSubmap(LinkTargets).GetOrAdd(targetKey, targetRef);
			if (resultTarget != targetRef)
				throw new Exception(String.Format("Link target has been set more than once.\n{0} -> \n\t{1}\n\t{2}", targetKey, resultTarget, targetRef));

			return context;
		}

		private static DataSubmap<string, string> LinkTargets = new DataSubmap<string, string>();
	}

	// =====================================================================
	#endregion
}
