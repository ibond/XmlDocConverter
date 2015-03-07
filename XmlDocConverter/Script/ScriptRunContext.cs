using NuDoq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocConverter.Fluent;

namespace XmlDocConverter
{
	/// <summary>
	/// The script context contains various pieces of data that we need to track during the running of a script.
	/// </summary>
	public class ScriptRunContext : IDisposable
	{
		/// <summary>
		/// Create the run context with an initial root context.
		/// </summary>
		/// <param name="initialRootContext">The root context that will be used each time an emit chain is started.</param>
		public ScriptRunContext(EmitContext<RootContext, EmitContext> initialRootContext = null)
		{
			// If we don't have a root context we create an empty one.
			m_initialEmitContext = initialRootContext ?? EmitContext.Create();
		}

		/// <summary>
		/// Add an object to the list of objects that should be automatically disposed.
		/// </summary>
		/// <param name="disposable">The disposable object.</param>
		public void AddAutoDisposeObject(IDisposable disposable)
		{
			Contract.Requires(disposable != null);

			lock (m_mutex)
			{
				m_autoDisposeObjects.Add(disposable);
			}
		}

		/// <summary>
		/// Add an object to the list of objects that should be automatically disposed.
		/// </summary>
		/// <param name="disposable">The disposable object.</param>
		public void RemoveAutoDisposeObject(IDisposable disposable)
		{
			lock (m_mutex)
			{
				// Remove only a single last item in case an item has been added multiple times.
				for (int i = m_autoDisposeObjects.Count - 1; i >= 0; --i)
				{
					if (m_autoDisposeObjects[i] == disposable)
					{
						m_autoDisposeObjects.RemoveAt(i);
						return;
					}
				}
			}
		}

		/// <summary>
		/// Implement the IDisposable interface.
		/// </summary>
		void IDisposable.Dispose()
		{
			lock (m_mutex)
			{
				// Write all of the emit targets.
				foreach (var target in m_emitTargets)
				{
					target.Target(target.WriterContext.OutputContext.Data);
				}
				m_emitTargets.Clear();

				// Get a unique list of auto-dispose object, keep them in the position where they were initially seen.
				var uniqueObjectList = new List<IDisposable>();
				var seenObjects = new HashSet<IDisposable>();
				foreach (var disposable in m_autoDisposeObjects)
				{
					if (seenObjects.Add(disposable))
						uniqueObjectList.Add(disposable);
				}

				// Clear our list of objects to avoid double-dispose.
				m_autoDisposeObjects.Clear();

				// Dispose of each of the objects.
				for (int i = uniqueObjectList.Count - 1; i >= 0; --i)
				{
					uniqueObjectList[i].Dispose();
				}
			}
		}

		/// <summary>
		/// Add a new emit target to be run when the script is complete.
		/// </summary>
		/// <param name="target">The emit target to be added.</param>
		/// <returns>true if the target is not already set to be run, false otherwise.</returns>
		public bool RegisterEmitTarget(EmitTargetContext target)
		{
			Contract.Requires(target != null);

			lock (m_mutex)
			{
				return m_emitTargets.Add(target);
			}
		}

		/// <summary>
		/// Remove an emit target.
		/// </summary>
		/// <param name="target">The emit target to be removed.</param>
		/// <returns>true if the target was removed, false if the target was not already added.</returns>
		public bool UnregisterEmitTarget(EmitTargetContext target)
		{
			lock (m_mutex)
			{
				return m_emitTargets.Remove(target);
			}
		}

		/// <summary>
		/// Get the file write streams.
		/// </summary>
		public ConcurrentDictionary<string, StreamWriter> FileWriteStreams { get { return m_fileWriteStreams; } }

		/// <summary>
		/// Get the initial emit context.
		/// </summary>
		public EmitContext<RootContext, EmitContext> InitialEmitContext { get { return m_initialEmitContext; } }
		
		/// <summary>
		/// The list of objects that should be automatically disposed after running the script.
		/// </summary>
		private readonly List<IDisposable> m_autoDisposeObjects = new List<IDisposable>();

		/// <summary>
		/// Track the emit targets that need to be run.
		/// </summary>
		private readonly HashSet<EmitTargetContext> m_emitTargets = new HashSet<EmitTargetContext>();

		/// <summary>
		/// The mutex for protecting this object.
		/// </summary>
		private readonly object m_mutex = new object();

		/// <summary>
		/// Track file write streams.
		/// </summary>
		private readonly ConcurrentDictionary<string, StreamWriter> m_fileWriteStreams = new ConcurrentDictionary<string, StreamWriter>();

		/// <summary>
		/// This is the initial emit context.  This is what is used when calling an Emit function.
		/// </summary>
		private readonly EmitContext<RootContext, EmitContext> m_initialEmitContext;
	}
}
