using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This is an interface for output data.
	/// </summary>
	public interface IOutputSource
	{
		/// <summary>
		/// Gets the source data as an enumerable of strings.
		/// </summary>
		IEnumerable<string> Data { get; }
	}

	/// <summary>
	/// An output source that simply wraps a list of strings.
	/// </summary>
	public class StringOutputSource : IOutputSource
	{
		/// <summary>
		/// Construct a StringOutputSource from a single string.
		/// </summary>
		/// <param name="value">The string returned from this output source.</param>
		public StringOutputSource(string value)
			: this(new string[] { value })
		{
		}

		/// <summary>
		/// Construct a StringOutputSource from an enumerable of strings.
		/// </summary>
		/// <param name="value">The string values returned from this output source.</param>
		public StringOutputSource(IEnumerable<string> values)
		{
			Contract.Requires(values != null);
			Contract.Requires(m_values != null);

			m_values = values;
		}

		/// <summary>
		/// Gets the source data as an enumerable of strings.
		/// </summary>
		public IEnumerable<string> Data { get { return m_values; } }

		/// <summary>
		/// The values in this output source.
		/// </summary>
		private readonly IEnumerable<string> m_values;
	}
}
