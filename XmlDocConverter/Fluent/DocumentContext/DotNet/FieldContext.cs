﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.Detail;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	public class MethodContext : MemberContext<MethodContext, MethodInfo>
	{
		public MethodContext(DocumentSource documentSource, MethodInfo info)
			: base(documentSource, info)
		{
		}

		/// <summary>
		/// The interface for an object that provides a method context.
		/// </summary>
		public interface IProvider
		{
			/// <summary>
			/// Get all methods.
			/// </summary>
			IEnumerable<MethodContext> GetMethods(BindingFlags bindingFlags);
		}
	}


	/// <summary>
	/// The context selector extensions for MethodContexts.
	/// </summary>
	public static class IMethodContextProviderExtensions
	{
		/// <summary>
		/// Select all of the members.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected member emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<MethodContext>, EmitContext<TDoc, TParent>>
			Methods<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, MethodContext.IProvider> selector,
				BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<MethodContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentAndParentContext(
					new DocumentContextCollection<MethodContext>(selector.DocumentContext.GetMethods(bindingFlags)),
					selector.EmitContext);
		}

		/// <summary>
		/// Select all of the classes from a document context collection.
		/// </summary>
		/// <param name="selector">The context selector object returned from EmitContext.Select.</param>
		/// <returns>The selected class emit contexts.</returns>
		public static EmitContext<DocumentContextCollection<MethodContext>, TParent>
			Members<TDoc, TParent>(
				this IContextSelector<TDoc, TParent, IDocumentContextCollection<MethodContext.IProvider>> selector,
				BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
			where TDoc : DocumentContext
			where TParent : EmitContext
		{
			Contract.Requires(selector != null);
			Contract.Ensures(Contract.Result<EmitContext<DocumentContextCollection<MethodContext>, EmitContext<TDoc, TParent>>>() != null);

			return selector.EmitContext.ReplaceDocumentContext(
				new DocumentContextCollection<MethodContext>(selector.DocumentContext.Elements.SelectMany(element => element.GetMethods(bindingFlags))));
		}
	}
}
