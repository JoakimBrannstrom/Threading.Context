using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threading.Context
{
	public static class ThreadPoolExtensions
	{
		public static void QueueUserWorkItemWithContext(Action action, string context)
		{
			var threadContext = new LogicalThreadContext();
			threadContext.Synchronize();

			var state = new Tuple<Action, string>(action, context);
			ThreadPool.QueueUserWorkItem(new WaitCallback(Run), state);
		}

		public static void QueueUserWorkItemWithId(Action action, int id)
		{
			var threadContext = new LogicalThreadContext();
			threadContext.Synchronize();

			var state = new Tuple<Action, int>(action, id);
			ThreadPool.QueueUserWorkItem(new WaitCallback(Run), state);
		}

		static void Run(object state)
		{
			var context = state as Tuple<Action, string>;
			var id = state as Tuple<Action, int>;
			var threadContext = new LogicalThreadContext();

			if (context != null)
			{
				var value = context.Item2;
				var message = string.Format("[{0:00}] Context, value: {1} ", Thread.CurrentThread.ManagedThreadId, value);
				Console.WriteLine(message);

				threadContext.Set("context", value);

				Thread.Sleep(500);
				var message2 = string.Format("[{0:00}] Context, starting action...", Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine(message2);

				context.Item1();
			}
			else if (id != null)
			{
				var logicalId = id.Item2;
				threadContext.Set("logicalId", logicalId);
				var message = string.Format("[{0:00}] [{1:00}] Action starting...", Thread.CurrentThread.ManagedThreadId, logicalId);
				Console.WriteLine(message);

				Thread.Sleep(500);

				id.Item1();
			}
		}
	}
}
