using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is the context that combines all emitted data and builds up the final output from a list of list of
	/// IOutputSource objects.
	/// </summary>
	public class EmitOutputContext : TextWriter, IOutputSource
	{
		/// <summary>
		/// Get the list of strings contained within this source.
		/// </summary>
		/// <returns>A list of strings for this source.</returns>
		public IEnumerable<string> Data
		{
			get
			{
				// Return a combined enumerable of all of our sources.
				return m_sources.SelectMany(source => source.Data);
			}
		}

		// TODO: This is hilariously inefficient...
		public override void Write(char value)
		{
			Write(new StringOutputSource(value.ToString()));
		}

		public void Write(IOutputSource source)
		{
			m_sources.Add(source);
		}

		// TODO: Allow setting encoding.
		public override Encoding Encoding
		{
			get { return Encoding.Default; }
		}

		/// <summary>
		/// The output sources that make up this output.
		/// </summary>
		private readonly List<IOutputSource> m_sources = new List<IOutputSource>();
	}
}
