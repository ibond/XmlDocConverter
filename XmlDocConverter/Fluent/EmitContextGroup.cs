using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This represents a group of emit contexts.
	/// </summary>
	/// <typeparam name="DocumentContextType">The type of the document contexts contained in this group.</typeparam>
	public class EmitContextGroup<DocumentContextType>
		where DocumentContextType : DocumentContext
	{
	}
}
