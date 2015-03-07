﻿using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuDoq;
using System.Reflection;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// This contains implementations for commonly used document sources.
	/// </summary>
	public static class StandardDocumentSources
	{
		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <returns>A new context with the updated assembly.</returns>
		public static EmitContext<RootContext, ParentEmitContextType> From<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, string assemblyPath)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			return context.From(new XmlDocPathPair[] { new XmlDocPathPair(assemblyPath, null) });
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="assemblyPath">The path to the assembly whose documentation should be converted.</param>
		/// <param name="xmlDocPath">The path to the XML document for the assembly.</param>
		/// <returns>A new context with the updated assembly.</returns>
		public static EmitContext<RootContext, ParentEmitContextType> From<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, string assemblyPath, string xmlDocPath)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			return context.From(new XmlDocPathPair[] { new XmlDocPathPair(assemblyPath, xmlDocPath) });
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="assemblyPaths">The path to the assemblies whose documentation should be converted.</param>
		/// <returns>A new context with the updated assemblies.</returns>
		public static EmitContext<RootContext, ParentEmitContextType> From<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, IEnumerable<string> assemblyPaths)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			return context.From(assemblyPaths.Select(assemblyPath => new XmlDocPathPair(assemblyPath)));
		}

		/// <summary>
		/// Construct a new EmitContext with the new doc source.
		/// </summary>
		/// <param name="context">The current emit context.</param>
		/// <param name="pathPairs">The pairs of document paths for each assembly and it's corresponding XML documentation.</param>
		/// <returns>A new context with the updated assemblies.</returns>
		public static EmitContext<RootContext, ParentEmitContextType> From<DocumentContextType, ParentEmitContextType>(this EmitContext<DocumentContextType, ParentEmitContextType> context, IEnumerable<XmlDocPathPair> pathPairs)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(pathPairs != null);
			Contract.Requires(Contract.ForAll(pathPairs, p => p != null));

			// Load the documents and pass them to the doc source.
			var assemblyMembers = pathPairs
				.Select(pair => DocReader.Read(Assembly.ReflectionOnlyLoadFrom(pair.AssemblyPath), pair.XmlDocPath))
				.ToImmutableList();

			return context.ReplaceDocumentContext(new RootContext(new DocumentSource(assemblyMembers)));
		}
	}
}
