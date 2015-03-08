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
	public class EmitFormatterContext
	{
		private EmitFormatterContext()
			: this(ImmutableDictionary<Type, FormatterExtension>.Empty)
		{
		}

		private EmitFormatterContext(ImmutableDictionary<Type, FormatterExtension> extensionMap)
		{
			Contract.Requires(extensionMap != null);
			Contract.Ensures(m_extensionMap != null);

			m_extensionMap = extensionMap;
		}

		public ExtensionType GetFormatterExtension<ExtensionType>(ExtensionType defaultExtension)
			where ExtensionType : FormatterExtension
		{
			FormatterExtension extension;
			if (!m_extensionMap.TryGetValue(defaultExtension.GetType(), out extension))
				extension = defaultExtension;

			return (ExtensionType)extension;
		}

		public EmitFormatterContext ReplaceFormatterExtension<ExtensionType>(ExtensionType newExtension)
			where ExtensionType : FormatterExtension
		{
			var newExtensionMap = m_extensionMap.SetItem(newExtension.GetType(), newExtension);
			return m_extensionMap != newExtensionMap
				? new EmitFormatterContext(m_extensionMap.SetItem(newExtension.GetType(), newExtension))
				: this;
		}

		private readonly ImmutableDictionary<Type, FormatterExtension> m_extensionMap;

		public static readonly EmitFormatterContext Default = new EmitFormatterContext();
	}
}
