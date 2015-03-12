using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This render filter applies a user-supplied function.
	/// </summary>
	public class RenderFilter : IRenderFilter
	{
		public RenderFilter(Func<IEnumerable<string>, IEnumerable<string>> filterFunction)
		{
			m_filterFunction = filterFunction;
		}

		public RenderFilter(Func<string, string> filterFunction)
			: this(strings => new string[] { filterFunction(string.Join("", strings)) })
		{
		}

		public IEnumerable<string> Apply(IEnumerable<string> source)
		{
			return m_filterFunction(source);
		}

		private readonly Func<IEnumerable<string>, IEnumerable<string>> m_filterFunction;


		public static readonly RenderFilter Identity = new RenderFilter((IEnumerable<string> data) => data);
		public static readonly RenderFilter Trim = new RenderFilter(str => str.Trim());

		public static readonly RenderFilter CollapseNewlines = new RenderFilter(
			str =>
			{
				// Single consecutive newlines collapse to a space.
				var result = Regex.Replace(str, @"(\S)[^\S\r\n]*(?:\r\n|\r|\n)[^\S\r\n]*(\S)", @"$1 $2");

				// Multiple consecutive newlines collapse to a single newline.
				result = Regex.Replace(result, @"(\S)(?:[^\S\r\n]*(\r\n|\r|\n)[^\S\r\n]*)+(\S)", @"$1$2$3");

				return result;
			});

		public static RenderFilter Replace(string oldValue, string newValue)
		{
			return new RenderFilter(str => str.Replace(oldValue, newValue));
		}

		public static RenderFilter RegexReplace(string pattern, string replacement)
		{
			return new RenderFilter(str => Regex.Replace(str, pattern, replacement));
		}

		public static RenderFilter Chain(params IRenderFilter[] filters)
		{
			return new RenderFilter(
				data =>
				{
					foreach (var filter in filters)
					{
						data = filter.Apply(data);
					}

					return data;
				});
		}
	}
}
