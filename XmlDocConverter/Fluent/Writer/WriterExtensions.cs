using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public struct WriteSelector<TDoc, TParent>
		where TDoc : DocumentContext
		where TParent : EmitContext
	{
		public WriteSelector(EmitContext<TDoc, TParent> context)
		{
			m_context = context;
		}

		public static EmitContext<TDoc, TParent> GetContext(WriteSelector<TDoc, TParent> selector)
		{
			return selector.m_context;
		}

		public static EmitOutputContext GetOutput(WriteSelector<TDoc, TParent> selector)
		{
			return selector.m_context.GetOutputContext();
		}

		private readonly EmitContext<TDoc, TParent> m_context;
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
		public static EmitContext<TDoc, TParent> Using<TDoc, TParent>(this EmitContext<TDoc, TParent> context, EmitFormatterContext formatter)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			return context.ReplaceFormatterContext(formatter);
		}
		

		public static EmitContext<TDoc, TParent> L<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			WriteSelector<TDoc, TParent>.GetOutput(selector).WriteLine();
			return WriteSelector<TDoc, TParent>.GetContext(selector);
		}

		public static EmitContext<TDoc, TParent> L<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector, string value)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			WriteSelector<TDoc, TParent>.GetOutput(selector).WriteLine(value);
			return WriteSelector<TDoc, TParent>.GetContext(selector);
		}

		public static EmitContext<TDoc, TParent> L<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector, string value, params object[] args)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{			
			WriteSelector<TDoc, TParent>.GetOutput(selector).WriteLine(string.Format(value, args));
			return WriteSelector<TDoc, TParent>.GetContext(selector);
		}


		public static EmitContext<TDoc, TParent> A<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector, string value)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			WriteSelector<TDoc, TParent>.GetOutput(selector).Write(value);
			return WriteSelector<TDoc, TParent>.GetContext(selector);
		}

		public static EmitContext<TDoc, TParent> A<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector, string value, params object[] args)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			WriteSelector<TDoc, TParent>.GetOutput(selector).Write(string.Format(value, args));
			return WriteSelector<TDoc, TParent>.GetContext(selector);
		}


		public static EmitContext<TDoc, TParent> Link<TDoc, TParent>(this WriteSelector<TDoc, TParent> selector, string targetKey, string contents)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			var context = WriteSelector<TDoc, TParent>.GetContext(selector);
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


		public static EmitContext<TDoc, TParent> SetLinkTarget<TDoc, TParent>(this EmitContext<TDoc, TParent> context, string targetKey, string targetRef)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{

			var resultTarget = context.GetPersistentDataMap().GetOrAdd(TargetKeyMap[targetKey], targetRef);
			if (resultTarget != (object)targetRef)
				throw new Exception(String.Format("Link target has been set more than once.\n{0} -> \n\t{1}\n\t{2}", targetKey, resultTarget, targetRef));

			return context;
		}

		private static Util.UniqueKeyMap<string> TargetKeyMap = new Util.UniqueKeyMap<string>();
	}
}
