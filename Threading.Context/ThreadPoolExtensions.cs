using System;
using System.Threading;

namespace Threading.Context
{
	public static class ThreadPoolExtensions
	{
		public static void QueueUserWorkItem(Action action)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(Run), action);
		}

		static void Run(object state)
		{
			var action = state as Action;
			if (action != null)
				action();
		}
	}
}
