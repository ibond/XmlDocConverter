using NuDoq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocConverter.Fluent
{
	class EmptyContainer : Container
	{
		public EmptyContainer()
			: base(Enumerable.Empty<Element>())
		{
		}

		public override TVisitor Accept<TVisitor>(TVisitor visitor)
		{
			// Do nothing here.
			return visitor;
		}
	}

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

			var classMap = new Dictionary<MemberInfo, Class>();
			var structMap = new Dictionary<MemberInfo, Struct>();

			// Build all of the data accessors.
			var visitor = new DelegateVisitor(new VisitorDelegates()
			{
				VisitClass = e => classMap.Add(e.Info, e),
				VisitStruct = e => structMap.Add(e.Info, e)
			});

			foreach (var member in m_assemblyMembers)
			{
				member.Accept(visitor);
			}
		}

		public Container GetTypeDocs(MemberInfo type)
		{
			foreach (var member in m_assemblyMembers)
			{
				var result = member.Elements.OfType<Member>().Where(m => m.Info == type).FirstOrDefault();
				if (result != null)
					return result;
			}

			return new EmptyContainer();
		}

		public Class GetClassDoc(Type type)
		{
			foreach (var member in m_assemblyMembers)
			{
				var result = member.Elements.OfType<Class>().Where(c => c.Info == type).FirstOrDefault();
				if (result != null)
					return result;
			}

			return null;
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
