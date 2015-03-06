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
	/// This type contains the context for how to emit documentation.
	/// </summary>
	public class EmitWriterContext
	{
		public EmitWriterContext()
		{
		}

		private EmitWriterContext(
			EmitWriterContext source,
			ImmutableDictionary<Type, WriteExtension> extensionMap)
		{
			WriteDocSource = source.WriteDocSource;
			WriteAssembly = source.WriteAssembly;
			WriteClass = source.WriteClass;
			WriteEnum = source.WriteEnum;
			m_extensionMap = extensionMap;
		}

		/// <summary>
		/// Write the given emit context.
		/// </summary>
		/// <param name="context">The context to be written.</param>
		/// <returns>The updated emit context of the same ItemType.</returns>
		public EmitContext Write(EmitContext context)
		{
			Contract.Requires(context != null);

			// Select the appropriate write function based on the context type.
			{
				var typedContext = context as EmitContext<DocSourceItem>;
				if(typedContext != null)
					return WriteDocSource(typedContext, typedContext.Item);
			}
			{
				var typedContext = context as EmitContext<AssemblyItem>;
				if(typedContext != null)
					return WriteAssembly(typedContext, typedContext.Item);
			}
			{
				var typedContext = context as EmitContext<ClassItem>;
				if(typedContext != null)
					return WriteClass(typedContext, typedContext.Item);
			}
			{
				var typedContext = context as EmitContext<EnumItem>;
				if(typedContext != null)
					return WriteEnum(typedContext, typedContext.Item);
			}
			
			// We couldn't find a match for this type.
			throw new ArgumentException("Invalid or unknown EmitContext item type specified.", "context");
		}
		
		public readonly Func<EmitContext<DocSourceItem>, DocSourceItem, EmitContext<DocSourceItem>> WriteDocSource =
			(context, item) =>
			{
				return context
					.Get.Assemblies
					.ForEach(emit =>
					{
						emit.Write();
					});
			};

		public readonly Func<EmitContext<AssemblyItem>, AssemblyItem, EmitContext<AssemblyItem>> WriteAssembly =
			(context, item) =>
			{
				return context
					.WriteHeader(item.Name)
					.SetHeaderLevel(2)
					.WriteHeader("TEST")
					.Scope(emit =>
						emit.SetHeaderLevel(5)
						.WriteHeader("Five"))
					.WriteHeader("TEST 2");
					//.WriteAssemblyName(item.Name);
					//.WriteHeader(item.Name)
					//.IndentHeader()
					//.DedentHeader();
			};

		public readonly Func<EmitContext<ClassItem>, ClassItem, EmitContext<ClassItem>> WriteClass =
			(context, item) =>
			{
				return context;
			};

		public readonly Func<EmitContext<EnumItem>, EnumItem, EmitContext<EnumItem>> WriteEnum =
			(context, item) =>
			{
				return context;
			};
		
		public ExtensionType GetWriteExtension<ExtensionType>(ExtensionType defaultExtension)
			where ExtensionType : WriteExtension
		{
			WriteExtension extension;
			if (!m_extensionMap.TryGetValue(defaultExtension.GetType(), out extension))
				extension = defaultExtension;

			return (ExtensionType)extension;
		}

		public EmitWriterContext SetWriteExtension<ExtensionType>(ExtensionType newExtension)
			where ExtensionType : WriteExtension
		{
			return new EmitWriterContext(this, m_extensionMap.SetItem(newExtension.GetType(), newExtension));
		}

		private readonly ImmutableDictionary<Type, WriteExtension> m_extensionMap = ImmutableDictionary<Type, WriteExtension>.Empty;
	}
}
