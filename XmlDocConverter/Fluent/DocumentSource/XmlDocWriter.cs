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
	public interface IXmlDocWriter
	{
		void Write(XNode node, EmitOutputContext output);
		void Write(XElement element, EmitOutputContext output);
		void Write(XText text, EmitOutputContext output);
	}

	public class XmlDocWriter : IXmlDocWriter
	{
		public XmlDocWriter()
		{
			m_tagWriters = ImmutableDictionary.Create<string, Action<XmlDocWriter, XElement, EmitOutputContext>>();
		}

		public XmlDocWriter(ImmutableDictionary<string, Action<XmlDocWriter, XElement, EmitOutputContext>> tagWriters)
		{
			m_tagWriters = tagWriters;
		}

		public virtual void Write(XNode node, EmitOutputContext output)
		{
			if (node is XText)
				Write((XText)node, output);
			else if (node is XElement)
				Write((XElement)node, output);
		}

		public virtual void Write(IEnumerable<XNode> nodes, EmitOutputContext output)
		{
			foreach (var node in nodes)
			{
				Write(node, output);
			}
		}

		public virtual void Write(XElement element, EmitOutputContext output)
		{
			Action<XmlDocWriter, XElement, EmitOutputContext> tagWriter;
			if (m_tagWriters.TryGetValue(element.Name.LocalName, out tagWriter))
			{
				tagWriter(this, element, output);
			}
			else
			{
				var trimmedNodes = element.Nodes()
					.Select(node =>
						{
							if (node.NodeType == XmlNodeType.Text)
							{
								if (node == element.FirstNode && node == element.LastNode)
									return new XText(((XText)node).Value.Trim());
								else if (node == element.FirstNode)
									return new XText(((XText)node).Value.TrimStart());
								else if (node == element.LastNode)
									return new XText(((XText)node).Value.TrimEnd());								
							}

							return node;
						});

				Write(trimmedNodes, output);
			}
		}

		public virtual void Write(XText text, EmitOutputContext output)
		{
			output.Write(text.Value);
		}

		private readonly ImmutableDictionary<string, Action<XmlDocWriter, XElement, EmitOutputContext>> m_tagWriters;
	}
}
