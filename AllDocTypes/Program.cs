using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllDocTypes
{
	/// <summary>
	/// The main program.
	/// </summary>
	/// <example>
	/// var x = new XYZ();
	/// </example>
	/// <exception cref="ArgumentNullException"></exception>
	public class Program
	{
		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
		}

		/// <summary>
		/// An unsafe method.
		/// </summary>
		/// <param name="ptr">The pointer</param>
		public unsafe static void TestUnsafe(
			int* ptr, 
			ref float r, 
			out float o,
			uint[] a,
			ref uint[,,,] a2,
			out uint[,,,,] a3,
			uint[][] a4,
			uint[,][][,,] a5, 
			out IEnumerable<IDictionary<IEnumerable<string>, string>> g,
			int*[] g3,
			int*[,] g4,
			int[] g5,
			int**** g6,
			ref int**[,,,] g7)
		{
			o = 0;
			g = null;
			a3 = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void Test(int x, float y = 0)
		{
		}
	}

	/// <summary>
	/// asdf
	/// </summary>
	unsafe public struct Test
	{
		/// <summary>
		/// f0
		/// </summary>
		public fixed int f0[20];

		/// <summary> 
		/// Enter description for operator. 
		/// ID string generated is "M:N.X.op_Explicit(N.X)~System.Int32".
		/// </summary> 
		/// <param name="x">Describe parameter.</param>
		/// <returns>Describe return value.</returns> 
		public static explicit operator int(Test x) { return 1; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public int op_Implicit(Test x) { return 3; }
	}
}
