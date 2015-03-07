using NuDoq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent.EmitContextExtensionSupport;

namespace XmlDocConverter.Fluent
{
	/// <summary>
	/// The root context for emitting documents.  This effectively contains a list of assemblies.
	/// </summary>
	public class RootContext : ScalarDocumentContext, IAssemblyContextProvider, IClassContextProvider
	{
		/// <summary>
		/// Construct an RootContext.
		/// </summary>
		/// <param name="documentSource">The source to be used for this context.</param>
		public RootContext(DocumentSource documentSource)
			: base(documentSource)
		{
		}

		/// <summary>
		/// Groups the items in the EmitContext by Assembly.
		/// </summary>
		public IEnumerable<AssemblyContext> Assemblies
		{
			get
			{
				return DocumentSource.AssemblyMembers.Select(source => new AssemblyContext(DocumentSource, source.Assembly));
			}
		}
		
		/// <summary>
		/// Groups the items in the EmitContext by class.
		/// </summary>
		public IEnumerable<ClassContext> Classes
		{
			get
			{
				return Assemblies.SelectMany(assembly => assembly.Classes);
			}
		}
	}
	

	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static class RootContextWriterExtensions
	{
		public delegate void WriteRootContextCollectionDelegate(EmitContext<DocumentContextCollection<RootContext>, EmitContext> context);
		public static readonly WriteRootContextCollectionDelegate WriteRootContextCollectionDefault = context => context.ForEach(emit => emit.Write());

		public static ParentEmitContextType Write<ParentEmitContextType>(this EmitContext<DocumentContextCollection<RootContext>, ParentEmitContextType> context)
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);

			// Get the write function from the emit context.
			var writeFunction = context.GetLocalData(WriteRootContextCollectionKey, WriteRootContextCollectionDefault);

			// Execute the write function.
			writeFunction(context.ReplaceParentContext((EmitContext)context.GetParentContext()));

			// This is a terminating collection function so we just return the parent.
			return context.GetParentContext();
		}

		/// <summary>
		/// Replace the writer for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<DocumentContextType, ParentEmitContextType>
			UsingRootContextCollectionWriter<DocumentContextType, ParentEmitContextType>(
				this EmitContext<DocumentContextType, ParentEmitContextType> context,
				WriteRootContextCollectionDelegate writer)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);

			return context.SetLocalData(WriteRootContextCollectionKey, writer ?? WriteRootContextCollectionDefault);
		}

		public delegate EmitContext<RootContext, EmitContext> WriteRootContextDelegate(EmitContext<RootContext, EmitContext> context);
		public static readonly WriteRootContextDelegate WriteRootContextDefault = context => context;
		
		public static EmitContext<RootContext, ParentEmitContextType> Write<ParentEmitContextType>(this EmitContext<RootContext, ParentEmitContextType> context)
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);

			// Get the write function from the emit context.
			var writeFunction = context.GetLocalData(WriteRootContextKey, WriteRootContextDefault);

			// Execute the write function.
			return writeFunction(context.ReplaceParentContext((EmitContext)context.GetParentContext()))
				.ReplaceParentContext(context.GetParentContext());
		}

		/// <summary>
		/// Replace the writer for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public static EmitContext<DocumentContextType, ParentEmitContextType> 
			UsingRootContextWriter<DocumentContextType, ParentEmitContextType>(
				this EmitContext<DocumentContextType, ParentEmitContextType> context,
				WriteRootContextDelegate writer)
			where DocumentContextType : DocumentContext
			where ParentEmitContextType : EmitContext
		{
			Contract.Requires(context != null);

			return context.SetLocalData(WriteRootContextKey, writer ?? WriteRootContextDefault);
		}
		
		/// <summary>
		/// The key for the root context write function.
		/// </summary>
		private static object WriteRootContextKey = new object();

		/// <summary>
		/// The key for the root context collection write function.
		/// </summary>
		private static object WriteRootContextCollectionKey = new object();
	}
}
