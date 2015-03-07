using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This represents a collection of emit contexts.
	/// </summary>
	/// <typeparam name="DocumentContextType">The type of the document contexts contained in this collection.</typeparam>
	public class EmitContextCollection<DocumentContextType> : IEnumerable<EmitContext<DocumentContextType>>
		where DocumentContextType : DocumentContext
	{
		public IEnumerator<EmitContext<DocumentContextType>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
