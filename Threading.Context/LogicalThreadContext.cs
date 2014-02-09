using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace Threading.Context
{
	public interface IThreadContext
	{
		T Get<T>(string key);
		void Set<T>(string key, T value);
		void Remove(string key);
	}

	public class ThreadContext : IThreadContext
	{
		[ThreadStatic]
		static Dictionary<string, object> _context;

		private static Dictionary<string, object> Context
		{
			get
			{
				return (_context = _context ?? new Dictionary<string, object>());
			}
		}

		public T Get<T>(string key)
		{
			if (!Context.ContainsKey(key))
				return default(T);

			return (T)Context[key];
		}

		public void Set<T>(string key, T value)
		{
			Context[key] = value;
		}

		public void Remove(string key)
		{
			Context.Remove(key);
		}
	}

	public class LogicalThreadContext : IThreadContext
	{
		public T Get<T>(string key)
		{
			return (T)CallContext.LogicalGetData(key);
		}

		public void Set<T>(string key, T value)
		{
			CallContext.LogicalSetData(key, value);
		}

		public void Remove(string key)
		{
			CallContext.FreeNamedDataSlot(key);
		}
	}
}
