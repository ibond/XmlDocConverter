using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XmlDocConverter.Fluent
{
	public class XmlDocWriter
	{
		public virtual void Write(XNode node, EmitOutputContext output)
		{
			if (node is XText)
				Write((XText)node, output);
			else if (node is XElement)
				Write((XElement)node, output);
		}

		public virtual void Write(XElement element, EmitOutputContext output)
		{
			foreach (var node in element.Nodes())
			{
				Write(node, output);
			}
		}

		public virtual void Write(XText text, EmitOutputContext output)
		{
			output.Write(text.Value);
		}
	}
}
