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
		void Set(string key, object value);
		void Remove(string key);
		void Synchronize();
	}

	public class LogicalThreadContext : IThreadContext
	{
		public T Get<T>(string key)
		{
			var value = GetValue(key);

			if (value is T)
				return (T)value;

			if(value != null)
				throw new InvalidCastException(value.GetType() + " could not be casted to " + typeof(T));

			return (T)value;
		}

		private object GetValue(string key)
		{
			var items = GetHttpContextItems();

			if (items != null)
				return items[key];
			else
				return CallContext.LogicalGetData(key);
		}

		public void Set(string key, object value)
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
