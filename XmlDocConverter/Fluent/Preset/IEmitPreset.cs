using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public interface IEmitPreset
	{
		EmitContext<TDoc> Apply<TDoc>(EmitContext<TDoc> context)
			where TDoc : DocumentContext;
	}

	public class EmitPreset : IEmitPreset
	{
		public EmitPreset(Func<EmitContextX, EmitContextX> apply)
		{
			m_apply = apply;
		}

		public EmitContext<TDoc> Apply<TDoc>(EmitContext<TDoc> context)
			where TDoc : DocumentContext
		{
			return m_apply(context)
				.ReplaceDocumentContext(context.Item);
		}

		private Func<EmitContextX, EmitContextX> m_apply;
	}

	public static class FormatterPresetExtensions
	{
		public static EmitContext<TDoc> Using<TDoc>(this EmitContext<TDoc> context, IEmitPreset preset)
			where TDoc : DocumentContext<TDoc>
		{
			Contract.Requires(context != null);
			Contract.Requires(preset != null);

			return preset.Apply(context);
		}
	}
}
