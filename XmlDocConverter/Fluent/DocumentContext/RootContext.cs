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
	public class RootContext : ScalarDocumentContext<RootContext>, IAssemblyContextProvider, IClassContextProvider
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

		public override Func<EmitContext<RootContext>, EmitContext> DefaultWriter
		{
			get { return context => context; }
		}
	}

	public class DocumentContextWriteExtension<TDocContext>
		where TDocContext : DocumentContext
	{
		public DocumentContextWriteExtension(WriteDelegate defaultDelegate)
		{
			Default = defaultDelegate;
		}

		public EmitContext<TDocContext, TParentContext> Write<TParentContext>(EmitContext<TDocContext, TParentContext> context)
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);

			// Get the write function from the emit context.
			var writeFunction = context.GetLocalData(DataKey, Default);

			// Execute the write function.  Reapply the parent context when we're done.
			return writeFunction(context)
				.ReplaceParentContext(context.GetParentContext());
		}

		/// <summary>
		/// Replace the writer for this emit context.
		/// </summary>
		/// <param name="context">The emit context.</param>
		/// <param name="writer">The formatter to be used for this context.</param>
		/// <returns>A new emit context with an updated formatter.</returns>
		public EmitContext<CurrentDocumentContextType, TParentContext>
			Using<CurrentDocumentContextType, TParentContext>(
				EmitContext<CurrentDocumentContextType, TParentContext> context,
				WriteDelegate writer)
			where CurrentDocumentContextType : DocumentContext
			where TParentContext : EmitContext
		{
			Contract.Requires(context != null);

			return context.UpdateLocalDataMap(map => map.SetItem(DataKey, writer ?? Default));
		}
		
		public delegate EmitContext<TDocContext> WriteDelegate(EmitContext<TDocContext> context);

		public readonly WriteDelegate Default;
		public readonly object DataKey = new object();
	}
	

	/// <summary>
	/// This provides the extensions for writing RootContext objects.
	/// </summary>
	public static class RootContextWriterExtensions
	{
		private static DocumentContextWriteExtension<RootContext> RootContextWriter = new DocumentContextWriteExtension<RootContext>(context => context);

		public static EmitContext<RootContext, TParentContext> Write<TParentContext>(this EmitContext<RootContext, TParentContext> context)
			where TParentContext : EmitContext
		{
			return RootContextWriter.Write(context);
		}

		public static EmitContext<TDocContext, TParentContext>
			UsingRootContextWriter<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context,
				DocumentContextWriteExtension<RootContext>.WriteDelegate writer)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return RootContextWriter.Using(context, writer);
		}


		private static DocumentContextWriteExtension<DocumentContextCollection<RootContext>> RootContextCollectionWriter = new DocumentContextWriteExtension<DocumentContextCollection<RootContext>>(context => context);

		public static EmitContext<DocumentContextCollection<RootContext>, TParentContext> Write<TParentContext>(this EmitContext<DocumentContextCollection<RootContext>, TParentContext> context)
			where TParentContext : EmitContext
		{
			return RootContextCollectionWriter.Write(context);
		}

		public static EmitContext<TDocContext, TParentContext>
			UsingRootContextCollectionWriter<TDocContext, TParentContext>(
				this EmitContext<TDocContext, TParentContext> context,
				DocumentContextWriteExtension<DocumentContextCollection<RootContext>>.WriteDelegate writer)
			where TDocContext : DocumentContext
			where TParentContext : EmitContext
		{
			return RootContextCollectionWriter.Using(context, writer);
		}


		//public delegate void WriteRootContextCollectionDelegate(EmitContext<DocumentContextCollection<RootContext>> context);
		//public static readonly WriteRootContextCollectionDelegate WriteRootContextCollectionDefault = context => context.ForEach(emit => { });

		//public static TParentContext Write<TParentContext>(this EmitContext<DocumentContextCollection<RootContext>, TParentContext> context)
		//	where TParentContext : EmitContext
		//{
		//	Contract.Requires(context != null);

		//	// Get the write function from the emit context.
		//	var writeFunction = context.GetLocalData(WriteRootContextCollectionKey, WriteRootContextCollectionDefault);

		//	// Execute the write function.
		//	writeFunction(context);

		//	// This is a terminating collection function so we just return the parent.
		//	return context.GetParentContext();
		//}

		///// <summary>
		///// Replace the writer for this emit context.
		///// </summary>
		///// <param name="context">The emit context.</param>
		///// <param name="writer">The formatter to be used for this context.</param>
		///// <returns>A new emit context with an updated formatter.</returns>
		//public static EmitContext<TDocContext, TParentContext>
		//	UsingRootContextCollectionWriter<TDocContext, TParentContext>(
		//		this EmitContext<TDocContext, TParentContext> context,
		//		WriteRootContextCollectionDelegate writer)
		//	where TDocContext : DocumentContext
		//	where TParentContext : EmitContext
		//{
		//	Contract.Requires(context != null);

		//	return context.SetLocalData(WriteRootContextCollectionKey, writer ?? WriteRootContextCollectionDefault);
		//}

		//public delegate EmitContext<RootContext> WriteRootContextDelegate(EmitContext<RootContext> context);
		//public static readonly WriteRootContextDelegate WriteRootContextDefault = context => context;
		
		//public static EmitContext<RootContext, TParentContext> Write<TParentContext>(this EmitContext<RootContext, TParentContext> context)
		//	where TParentContext : EmitContext
		//{
		//	Contract.Requires(context != null);

		//	// Get the write function from the emit context.
		//	var writeFunction = context.GetLocalData(WriteRootContextKey, WriteRootContextDefault);

		//	// Execute the write function.
		//	return writeFunction(context)
		//		.ReplaceParentContext(context.GetParentContext());
		//}

		///// <summary>
		///// Replace the writer for this emit context.
		///// </summary>
		///// <param name="context">The emit context.</param>
		///// <param name="writer">The formatter to be used for this context.</param>
		///// <returns>A new emit context with an updated formatter.</returns>
		//public static EmitContext<TDocContext, TParentContext> 
		//	UsingRootContextWriter<TDocContext, TParentContext>(
		//		this EmitContext<TDocContext, TParentContext> context,
		//		WriteRootContextDelegate writer)
		//	where TDocContext : DocumentContext
		//	where TParentContext : EmitContext
		//{
		//	Contract.Requires(context != null);

		//	return context.SetLocalData(WriteRootContextKey, writer ?? WriteRootContextDefault);
		//}
		
		///// <summary>
		///// The key for the root context write function.
		///// </summary>
		//private static object WriteRootContextKey = new object();

		///// <summary>
		///// The key for the root context collection write function.
		///// </summary>
		//private static object WriteRootContextCollectionKey = new object();
	}
}
