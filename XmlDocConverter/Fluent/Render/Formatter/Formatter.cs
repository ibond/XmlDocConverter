using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public abstract class FormatterEntry
	{
	}

	public class FormatterEntry<TFormatter> : FormatterEntry
		where TFormatter : Formatter<TFormatter>, new()
	{
		public FormatterEntry(TFormatter data, Formatter<TFormatter>.Writer writer)
		{
			Data = data;
			Writer = writer;
		}

		public EmitContext<TDoc> Format<TDoc>(EmitContext<TDoc> context, FormatterContentSource source)
			where TDoc : DocumentContext
		{
			return Writer(context, Data, source.OutputSource)
				.ReplaceDocumentContext(context.Item);
		}

		public readonly TFormatter Data;
		public readonly Formatter<TFormatter>.Writer Writer;
	}

	public abstract class Formatter
	{
		private static DataSubmap<Type, FormatterEntry> FormattersMap = new DataSubmap<Type, FormatterEntry>();

		public static FormatterEntry<TFormatter> GetFormatter<TFormatter>(EmitContextX context)
			where TFormatter : Formatter<TFormatter>, new()
		{
			return context
				.GetLocalData(
					FormattersMap,
					typeof(TFormatter),
					k => new FormatterEntry<TFormatter>(Formatter<TFormatter>.Default, (c, f, s) => { c.GetOutputContext().Write(s); return c; }));
		}

		public static EmitContext<TDoc> SetWriter<TDoc, TFormatter>(EmitContext<TDoc> context, Formatter<TFormatter>.Writer formatWriter)
			where TDoc : DocumentContext
			where TFormatter : Formatter<TFormatter>, new()
		{
			return context
				.UpdateLocalDataMap(FormattersMap, map => map.SetItem(typeof(TFormatter), new FormatterEntry<TFormatter>(GetFormatter<TFormatter>(context).Data, formatWriter)));
		}

		public static EmitWriteContext<TDoc> Format<TFormatter, TDoc>(EmitWriteContext<TDoc> context, FormatterContentSource source)
			where TDoc : DocumentContext
			where TFormatter : Formatter<TFormatter>, new()
		{
			return GetFormatter<TFormatter>(context)
				.Format(context, source)
				.Write;
		}
	}

	public abstract class Formatter<TDerived> : Formatter
		where TDerived : Formatter<TDerived>, new()
	{
		public delegate EmitContextX Writer(EmitContextX context, TDerived formatData, IOutputSource source);
		
		private TDerived Data
		{
			get
			{
				return (TDerived)this;
			}
		}

		public static TDerived Default = new TDerived();
	}


	public static class FormatterExtension
	{
		public static EmitContext<TDoc> Using<TDoc, TFormatter>(this EmitContext<TDoc> context, Formatter<TFormatter>.Writer formatWriter)
			where TDoc : DocumentContext<TDoc>
			where TFormatter : Formatter<TFormatter>, new()
		{
			return Formatter.SetWriter(context, formatWriter);
		}
	}
}
