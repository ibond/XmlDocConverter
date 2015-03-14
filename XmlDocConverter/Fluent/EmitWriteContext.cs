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
			EmitWriterContext writerContext)
			: base(documentContext, persistentDataMap, localDataMap, writerContext)
		{
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


		public static EmitWriteContext<TDoc> Link<TDoc>(this EmitWriteContext<TDoc> context, string targetKey, string contents)
			where TDoc : DocumentContext
		{
			return context
				.WithFilter(
					new RenderFilter((string data) =>
					{
						object targetRef;
						if (context.GetPersistentDataMap().TryGetValue(TargetKeyMap[targetKey], out targetRef))
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

			var resultTarget = context.GetPersistentDataMap().GetOrAdd(TargetKeyMap[targetKey], targetRef);
			if (resultTarget != (object)targetRef)
				throw new Exception(String.Format("Link target has been set more than once.\n{0} -> \n\t{1}\n\t{2}", targetKey, resultTarget, targetRef));

			return context;
		}

		private static Util.UniqueKeyMap<string> TargetKeyMap = new Util.UniqueKeyMap<string>();
	}

	// =====================================================================
	#endregion
}
