using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Collections;

namespace Threading.Context
{
	// http://forum.springframework.net/showthread.php?572-CallContext-vs-ThreadStatic-vs-HttpContext
	// http://piers7.blogspot.se/2005/11/threadstatic-callcontext-and_02.html

	public interface IThreadContext
	{
		T Get<T>(string key);
		void Set<T>(string key, T value);
		void Remove(string key);
		void Synchronize();
	}

	public class BuggyThreadContext : IThreadContext
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

		public void Synchronize()
		{
		}
	}

	public class LogicalThreadContext : IThreadContext
	{
		public T Get<T>(string key)
		{
			var items = GetHttpContextItems();

			if (items != null)
				return (T)items[key];
			else
				return (T)CallContext.LogicalGetData(key);
		}

		public void Set<T>(string key, T value)
		{
			var items = GetHttpContextItems();
			if(items != null)
				items[key] = value;
			else
				CallContext.LogicalSetData(key, value);
		}

		public void Remove(string key)
		{
			CallContext.FreeNamedDataSlot(key);
		}

		public void Synchronize()
		{
			var items = GetHttpContextItems();
			if (items == null)
				return;

			var e = items.GetEnumerator();
			while(e.MoveNext())
				CallContext.LogicalSetData((string)e.Key, e.Value);
		}

		protected virtual IDictionary GetHttpContextItems()
		{
			var context = HttpContext.Current;
			if(context != null)
				return context.Items;
			return null;
		}
	}
}
