using NuDoq;
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
	/// This class contains the data and functionality for document source data.
	/// </summary>
	public class DocumentSource
	{
		/// <summary>
		/// Construct a DocumentSource.
		/// </summary>
		/// <param name="assemblyMembers">The assembly member sources to be used for this document source.</param>
		public DocumentSource(ImmutableList<AssemblyMembers> assemblyMembers)
		{
			Contract.Requires(assemblyMembers != null);
			Contract.Requires(Contract.ForAll(assemblyMembers, a => a != null));
			Contract.Ensures(this.m_assemblyMembers != null);

			m_assemblyMembers = assemblyMembers;
		}

		/// <summary>
		/// Get the assembly members for this source.
		/// </summary>
		public ImmutableList<AssemblyMembers> AssemblyMembers { get { return m_assemblyMembers; } }

		/// <summary>
		/// The assembly members for this doc source.
		/// </summary>
		private readonly ImmutableList<AssemblyMembers> m_assemblyMembers;
	}
}
