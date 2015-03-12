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
	/// Static MemberContext values separate from any particular type.
	/// </summary>
	public static class MemberContext
	{
		/// <summary>
		/// The default set of binding flags when getting members.
		/// </summary>
		public const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;
	}

	/// <summary>
	/// A base context for .NET reflection info types.
	/// </summary>
	public abstract class MemberContext<TDerived, TInfo> : DotNetDocumentContext<TDerived>, DocEntryContext.IProvider
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
		/// Get the documentation entry for this member.
		/// </summary>
		/// <returns>The doc entry for this member.</returns>
		public DocEntryContext GetDocEntry()
		{
			return new DocEntryContext(DocumentSource, DocumentSource.TryGetEntry(m_info));
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
