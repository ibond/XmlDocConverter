using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;
using XmlDocConverter.Fluent.Detail;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// A base context for .NET reflection info types.
	/// </summary>
	public abstract class MemberContext<TDerived, TInfo> : DotNetDocumentContext<TDerived>
		where TDerived : MemberContext<TDerived, TInfo>
		where TInfo : MemberInfo
	{
		/// <summary>
		/// Construct an MemberContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public MemberContext(DocumentSource documentSource, TInfo info)
			: base(documentSource)
		{
			Contract.Requires(info != null);
			Contract.Ensures(m_info != null);

			m_info = info;
		}

		/// <summary>
		/// Gets the name of this member.
		/// </summary>
		public string Name { get { return m_info.Name; } }

		/// <summary>
		/// Gets the member info.
		/// </summary>
		public TInfo Info { get { return m_info; } }

		/// <summary>
		/// The info for this member.
		/// </summary>
		private readonly TInfo m_info;
	}
}
