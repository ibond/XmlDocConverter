using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	/// This is an interface for output data.
	/// </summary>
	public interface IOutputSource
	{
		IEnumerable<string> GetData();
	}

	/// <summary>
	/// This is the context that combines all emitted data and builds up the final output from a list of list of
	/// IOutputSource objects.
	/// </summary>
	public class EmitOutputContext : IOutputSource
	{
		/// <summary>
		/// Get the list of strings contained within this source.
		/// </summary>
		/// <returns>A list of strings for this source.</returns>
		public IEnumerable<string> GetData()
		{
			// Return a combined enumerable of all of our sources.
			return m_sources.SelectMany(source => source.GetData());
		}

		/// <summary>
		/// The output sources that make up this output.
		/// </summary>
		private readonly List<IOutputSource> m_sources = new List<IOutputSource>();
	}

	/// <summary>
	/// This type contains the context for how to emit documentation.
	/// </summary>
	public class EmitFormatterContext
	{
		public EmitFormatterContext()
		{
		}

		//private EmitFormatterContext(
		//	EmitFormatterContext source,
		//	ImmutableDictionary<Type, WriteExtension> extensionMap)
		//{
		//	WriteDocSource = source.WriteDocSource;
		//	WriteAssembly = source.WriteAssembly;
		//	WriteClass = source.WriteClass;
		//	WriteEnum = source.WriteEnum;
		//	m_extensionMap = extensionMap;
		//}

		///// <summary>
		///// Write the given emit context.
		///// </summary>
		///// <param name="context">The context to be written.</param>
		///// <returns>The updated emit context of the same ItemType.</returns>
		//public EmitContext Write(EmitContext context)
		//{
		//	Contract.Requires(context != null);

		//	// Select the appropriate write function based on the context type.
		//	{
		//		var typedContext = context as EmitContext<DocSourceItem>;
		//		if(typedContext != null)
		//			return WriteDocSource(typedContext, typedContext.Item);
		//	}
		//	{
		//		var typedContext = context as EmitContext<AssemblyItem>;
		//		if(typedContext != null)
		//			return WriteAssembly(typedContext, typedContext.Item);
		//	}
		//	{
		//		var typedContext = context as EmitContext<ClassItem>;
		//		if(typedContext != null)
		//			return WriteClass(typedContext, typedContext.Item);
		//	}
		//	{
		//		var typedContext = context as EmitContext<EnumItem>;
		//		if(typedContext != null)
		//			return WriteEnum(typedContext, typedContext.Item);
		//	}
			
		//	// We couldn't find a match for this type.
		//	throw new ArgumentException("Invalid or unknown EmitContext item type specified.", "context");
		//}
		
		//public readonly Func<EmitContext<DocSourceItem>, DocSourceItem, EmitContext<DocSourceItem>> WriteDocSource =
		//	(context, item) =>
		//	{
		//		return context
		//			.Get.Assemblies
		//			.ForEach(emit =>
		//			{
		//				emit.Write();
		//			});
		//	};

		//public readonly Func<EmitContext<AssemblyItem>, AssemblyItem, EmitContext<AssemblyItem>> WriteAssembly =
		//	(context, item) =>
		//	{
		//		return context
		//			.WriteHeader(item.Name)
		//			.SetHeaderLevel(2)
		//			.WriteHeader("TEST")
		//			.Scope(emit =>
		//				emit.SetHeaderLevel(5)
		//				.WriteHeader("Five"))
		//			.WriteHeader("TEST 2");
		//			//.WriteAssemblyName(item.Name);
		//			//.WriteHeader(item.Name)
		//			//.IndentHeader()
		//			//.DedentHeader();
		//	};

		//public readonly Func<EmitContext<ClassItem>, ClassItem, EmitContext<ClassItem>> WriteClass =
		//	(context, item) =>
		//	{
		//		return context;
		//	};

		//public readonly Func<EmitContext<EnumItem>, EnumItem, EmitContext<EnumItem>> WriteEnum =
		//	(context, item) =>
		//	{
		//		return context;
		//	};
		
		//public ExtensionType GetWriteExtension<ExtensionType>(ExtensionType defaultExtension)
		//	where ExtensionType : WriteExtension
		//{
		//	WriteExtension extension;
		//	if (!m_extensionMap.TryGetValue(defaultExtension.GetType(), out extension))
		//		extension = defaultExtension;

		//	return (ExtensionType)extension;
		//}

		//public EmitFormatterContext SetWriteExtension<ExtensionType>(ExtensionType newExtension)
		//	where ExtensionType : WriteExtension
		//{
		//	return new EmitFormatterContext(this, m_extensionMap.SetItem(newExtension.GetType(), newExtension));
		//}

		//private readonly ImmutableDictionary<Type, WriteExtension> m_extensionMap = ImmutableDictionary<Type, WriteExtension>.Empty;

		public static readonly EmitFormatterContext Default = new EmitFormatterContext();
	}

	/// <summary>
	/// The writer context is a combination of output context and formatter context.  This defines how the output should
	/// look and where it should go.
	/// </summary>
	public class EmitWriterContext
	{
		/// <summary>
		/// Construct a writer context using the default formatter.
		/// </summary>
		public EmitWriterContext()
			:this(new EmitOutputContext(), EmitFormatterContext.Default)
		{
		}

		/// <summary>
		/// Construct a new writer context with the given output and formatter contexts.
		/// </summary>
		/// <param name="outputContext">The output context to use for this writer.</param>
		/// <param name="formatterContext">The formatter context to use for this writer.</param>
		private EmitWriterContext(EmitOutputContext outputContext, EmitFormatterContext formatterContext)
		{
			Contract.Requires(outputContext != null);
			Contract.Requires(formatterContext != null);
			Contract.Ensures(OutputContext != null);
			Contract.Ensures(FormatterContext != null);

			OutputContext = outputContext;
			FormatterContext = formatterContext;
		}

		/// <summary>
		/// Create a new writer context based on the existing context.
		/// </summary>
		/// <returns>A new writer context with an empty output context and a fresh formatter context.</returns>
		public EmitWriterContext CreateNew()
		{
			Contract.Requires(Contract.Result<EmitWriterContext>() != null);

			// TODO: Reset the formatter context.
			return new EmitWriterContext(new EmitOutputContext(), FormatterContext);
		}

		/// <summary>
		/// Replace the formatter for this writer context.
		/// </summary>
		/// <param name="formatter">The formatter to be used for this context.</param>
		/// <returns>A new writer context with an updated formatter.</returns>
		public EmitWriterContext ReplaceFormatterContext(EmitFormatterContext formatter)
		{
			Contract.Requires(formatter != null);
			Contract.Requires(Contract.Result<EmitWriterContext>() != null);

			return new EmitWriterContext(OutputContext, formatter);
		}

		/// <summary>
		/// The output context for this target.
		/// </summary>
		public readonly EmitOutputContext OutputContext;

		/// <summary>
		/// The formatter context.
		/// </summary>
		public readonly EmitFormatterContext FormatterContext;
	}

	/// <summary>
	/// This is the context for an emit target.
	/// </summary>
	public class EmitTargetContext
	{
		/// <summary>
		/// Construct a new target context.
		/// </summary>
		/// <param name="writerContext">The writer context for this target.</param>
		/// <param name="target">The function to be called when we should write the data.</param>
		public EmitTargetContext(EmitWriterContext writerContext, WriteDataToTargetDelegate target)
		{
			Contract.Requires(writerContext != null);
			Contract.Requires(target != null);
			Contract.Ensures(WriterContext != null);
			Contract.Ensures(Target != null);

			WriterContext = writerContext;
			Target = target;
		}

		/// <summary>
		/// The writer context for this target.
		/// </summary>
		public readonly EmitWriterContext WriterContext;

		/// <summary>
		/// The target to which we should write data when we are done.
		/// </summary>
		public readonly WriteDataToTargetDelegate Target;
	}
}
