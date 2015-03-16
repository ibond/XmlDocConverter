using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XmlDocConverter.Fluent
{
	public class XmlDocWriter
	{
		public delegate EmitContextX TagWriter(XmlDocWriter writer, XElement element, EmitContextX context);

		public static readonly ImmutableDictionary<string, TagWriter> DefaultTagWriters;

		static XmlDocWriter()
		{
			var builder = ImmutableDictionary.CreateBuilder<string, TagWriter>();

			builder.Add("c", (writer, element, context) => context.Write.InlineCode(writer.TrimElement(element).Value));
			builder.Add("code", (writer, element, context) => context.Write.Code(writer.TrimElement(element).Value));

			DefaultTagWriters = builder.ToImmutableDictionary();
		}
		
		public XmlDocWriter()
			:this(DefaultTagWriters)
		{
		}

		public XmlDocWriter(ImmutableDictionary<string, TagWriter> tagWriters)
		{
			m_tagWriters = tagWriters;
		}

		public virtual EmitContextX Write(XNode node, EmitContextX context)
		{
			if (node is XText)
				return Write((XText)node, context);
			else if (node is XElement)
				return Write((XElement)node, context);

			return context;
		}

		public virtual EmitContextX Write(IEnumerable<XNode> nodes, EmitContextX context)
		{
			foreach (var node in nodes)
			{
				context = Write(node, context);
			}
			return context;
		}

		public virtual EmitContextX Write(XElement element, EmitContextX context)
		{
			TagWriter tagWriter;
			if (m_tagWriters.TryGetValue(element.Name.LocalName, out tagWriter))
			{
				return tagWriter(this, element, context);
			}
			else
			{
				return Write(TrimElement(element).Nodes(), context);
			}
		}

		public virtual XElement TrimElement(XElement element)
		{
			var newElement = new XElement(element);

			foreach (var node in newElement.Nodes())
			{
				if (node.NodeType == XmlNodeType.Text)
				{
					if (node == newElement.FirstNode && node == newElement.LastNode)
						node.ReplaceWith(new XText(((XText)node).Value.Trim()));
					else if (node == newElement.FirstNode)
						node.ReplaceWith(new XText(((XText)node).Value.TrimStart()));
					else if (node == newElement.LastNode)
						node.ReplaceWith(new XText(((XText)node).Value.TrimEnd()));
				}
			}

			return newElement;
		}

		public virtual EmitContextX Write(XText text, EmitContextX context)
		{
			return context.Write.A(text.Value);
		}

		private readonly ImmutableDictionary<string, TagWriter> m_tagWriters;
	}
}
