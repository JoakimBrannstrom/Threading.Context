using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Threading.Context
{
	// http://forum.springframework.net/showthread.php?572-CallContext-vs-ThreadStatic-vs-HttpContext
	// http://piers7.blogspot.se/2005/11/threadstatic-callcontext-and_02.html

	public interface IThreadContext
	{
		T Get<T>(string key);
		void Set(string key, object value);
		void Remove(string key);
	}

	public class LogicalThreadContext : IThreadContext
	{
		public T Get<T>(string key)
		{
			var items = GetHttpContextItems();

			if (items != null)
				return (T)items[key];

			return (T)CallContext.LogicalGetData(key);
		}

		public void Set(string key, object value)
		{
			var items = GetHttpContextItems();
			if(items != null)
				items[key] = value;

			CallContext.LogicalSetData(key, value);
		}

		public void Remove(string key)
		{
			var items = GetHttpContextItems();
			if (items != null)
				items.Remove(key);

			CallContext.FreeNamedDataSlot(key);
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
