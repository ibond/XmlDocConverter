using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public struct WriteSelector<TDoc>
		where TDoc : DocumentContext
	{
		public WriteSelector(EmitContext<TDoc> context)
		{
			m_context = context;
		}

		public static EmitContext<TDoc> GetContext(WriteSelector<TDoc> selector)
		{
			return selector.m_context;
		}

		public static EmitOutputContext GetOutput(WriteSelector<TDoc> selector)
		{
			return selector.m_context.GetOutputContext();
		}

		private readonly EmitContext<TDoc> m_context;
	}

	/// <summary>
	/// Extension methods to make using writers more fluent.
	/// </summary>
	public static class WriterExtensions
	{
		/// <summary>
		/// Replace the formatter for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<TDoc> Using<TDoc>(this EmitContext<TDoc> context, EmitFormatterContext formatter)
			where TDoc : DocumentContext
		{
			return context.ReplaceFormatterContext(formatter);
		}
		

		public static EmitContext<TDoc> L<TDoc>(this WriteSelector<TDoc> selector)
			where TDoc : DocumentContext
		{
			WriteSelector<TDoc>.GetOutput(selector).WriteLine();
			return WriteSelector<TDoc>.GetContext(selector);
		}

		public static EmitContext<TDoc> L<TDoc>(this WriteSelector<TDoc> selector, string value)
			where TDoc : DocumentContext
		{
			WriteSelector<TDoc>.GetOutput(selector).WriteLine(value);
			return WriteSelector<TDoc>.GetContext(selector);
		}

		public static EmitContext<TDoc> L<TDoc>(this WriteSelector<TDoc> selector, string value, params object[] args)
			where TDoc : DocumentContext
		{			
			WriteSelector<TDoc>.GetOutput(selector).WriteLine(string.Format(value, args));
			return WriteSelector<TDoc>.GetContext(selector);
		}


		public static EmitContext<TDoc> A<TDoc>(this WriteSelector<TDoc> selector, string value)
			where TDoc : DocumentContext
		{
			WriteSelector<TDoc>.GetOutput(selector).Write(value);
			return WriteSelector<TDoc>.GetContext(selector);
		}

		public static EmitContext<TDoc> A<TDoc>(this WriteSelector<TDoc> selector, string value, params object[] args)
			where TDoc : DocumentContext
		{
			WriteSelector<TDoc>.GetOutput(selector).Write(string.Format(value, args));
			return WriteSelector<TDoc>.GetContext(selector);
		}


		public static EmitContext<TDoc> Link<TDoc>(this WriteSelector<TDoc> selector, string targetKey, string contents)
			where TDoc : DocumentContext
		{
			var context = WriteSelector<TDoc>.GetContext(selector);
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
					emit => emit.Write.A(contents));
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
}
