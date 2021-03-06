﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// An interface type for a document context collection.  This allows us to use covariance with document context
	/// collections.
	/// </summary>
	/// <typeparam name="TDoc">The type of each element in this collection.</typeparam>
	public interface IDocumentContextCollection<out TDoc>
	{
		/// <summary>
		/// Get the document context elements.
		/// </summary>
		IEnumerable<TDoc> Elements { get; }
	}


	/// <summary>
	/// A collection of document contexts.
	/// </summary>
	/// <typeparam name="TDoc">The type of each element in this collection.</typeparam>
	public class DocumentContextCollection<TDoc> : DocumentContext<DocumentContextCollection<TDoc>>, IDocumentContextCollection<TDoc>
		where TDoc : DocumentContext<TDoc>
	{
		/// <summary>
		/// Construct a document context collection.
		/// </summary>
		/// <param name="elements">The elements contained in this collection.</param>
		public DocumentContextCollection(IEnumerable<TDoc> elements)
		{
			Contract.Requires(elements != null);
			Contract.Requires(Contract.ForAll(elements, e => e != null));
			Contract.Ensures(m_elements != null);

			m_elements = elements;
		}

		/// <summary>
		/// The default writer for a document context collection just writes each element.
		/// </summary>
		protected override EmitWriter<DocumentContextCollection<TDoc>>.Writer GetDefaultRenderer()
		{
			return item => item.Emit.ForEach(element => element.Render());
		}
		
		/// <summary>
		/// Gets the elements in this collection.
		/// </summary>
		public IEnumerable<TDoc> Elements { get { return m_elements; } }

		/// <summary>
		/// The elements in this collection.
		/// </summary>
		private readonly IEnumerable<TDoc> m_elements;
	}

	public static class DocumentContextCollectionExtensions
	{
		public static bool Any<TDocElement>(this EmitContext<DocumentContextCollection<TDocElement>> context)
			where TDocElement : DocumentContext<TDocElement>
		{
			Contract.Requires(context != null);

			return context.GetDocumentContext().Elements.Any();
		}


		public static EmitContext<DocumentContextCollection<TDocElement>>
			IfAny<TDocElement>(
				this EmitContext<DocumentContextCollection<TDocElement>> context, 
				Action<EmitContext<DocumentContextCollection<TDocElement>>> action)
			
			where TDocElement : DocumentContext<TDocElement>
		{
			Contract.Requires(context != null);
			Contract.Requires(action != null);

			if (context.Any())
				action(context);

			return context;
		}
	}
}
