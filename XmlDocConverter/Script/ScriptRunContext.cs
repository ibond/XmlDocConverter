using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		/// Add an object to the list of objects that should be automatically disposed.
		/// </summary>
		/// <param name="disposable">The disposable object.</param>
		public void AddAutoDisposeObject(IDisposable disposable)
		{
			Contract.Requires(disposable != null);

			lock (m_autoDisposeObjectsMutex)
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
			lock (m_autoDisposeObjectsMutex)
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
			var uniqueObjectList = new List<IDisposable>();
			lock (m_autoDisposeObjectsMutex)
			{
				// Get a unique list of auto-dispose object, keep them in the position where they were initially seen.
				var seenObjects = new HashSet<IDisposable>();
				foreach (var disposable in m_autoDisposeObjects)
				{
					if (seenObjects.Add(disposable))
						uniqueObjectList.Add(disposable);
				}

				// Clear our list of objects to avoid double-dispose.
				m_autoDisposeObjects.Clear();
			}

			// Dispose of each of the objects.
			for (int i = uniqueObjectList.Count - 1; i >= 0; --i)
			{
				uniqueObjectList[i].Dispose();
			}
		}

		/// <summary>
		/// Get the file write streams.
		/// </summary>
		public ConcurrentDictionary<string, StreamWriter> FileWriteStreams { get { return m_fileWriteStreams; } }
		
		/// <summary>
		/// The list of objects that should be automatically disposed after running the script.
		/// </summary>
		private readonly List<IDisposable> m_autoDisposeObjects = new List<IDisposable>();

		/// <summary>
		/// The mutex for protecting m_autoDisposeObjects.
		/// </summary>
		private readonly object m_autoDisposeObjectsMutex = new object();

		/// <summary>
		/// Track file write streams.
		/// </summary>
		private readonly ConcurrentDictionary<string, StreamWriter> m_fileWriteStreams = new ConcurrentDictionary<string, StreamWriter>();
	}
}
