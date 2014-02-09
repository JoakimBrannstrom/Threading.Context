using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Threading.Context.UnitTests
{
	[TestClass]
	public class ThreadPoolExtensionTests
	{
		const string ItemKey = "Item";
		IThreadContext _context = new LogicalThreadContext();

		[TestCleanup]
		public void Cleanup()
		{
			_context.Remove(ItemKey);
		}

		[TestMethod]
		public void GivenNoInitialState_WhenItemIsFetchedFromThreadPoolThread_ThenTheResultShouldBeNull()
		{
			// Arrange
			Console.WriteLine(string.Format("[{0}] Main", Thread.CurrentThread.ManagedThreadId));

			string item = null;
			Action<string> action = (value) =>
			{
				if (value != null && item == null)
					item = value;
			};

			// Act
			ThreadPoolExtensions.QueueUserWorkItemWithId(() => RunInBackground(action), 0);

			var retries = 0;
			while (retries < 100)
			{
				retries++;
				Thread.Sleep(10);
			}

			// Assert
			Assert.IsNull(item);
		}

		[TestMethod]
		public void GivenItemIsStoredInContext_WhenItemIsFetchedFromThreadPoolThread_ThenTheResultShouldBeExpectedItem()
		{
			// Arrange
			Console.WriteLine(string.Format("[{0}] Main", Thread.CurrentThread.ManagedThreadId));

			var expectedItem = "Item is stored";
			string item = null;
			Action<string> action = (value) =>
			{
				if (value != null && item == null)
					item = value;
			};

			_context.Set<string>(ItemKey, expectedItem);

			// Act
			ThreadPoolExtensions.QueueUserWorkItemWithId(() => RunInBackground(action), 0);

			var retries = 0;
			while (retries < 100)
			{
				retries++;
				Thread.Sleep(10);
			}

			// Assert
			Console.WriteLine("item: " + item);
			Assert.AreEqual(expectedItem, item);
		}

		private void RunInBackground(Action<string> action)
		{
			var threadContext = new LogicalThreadContext();
			var logicalId = threadContext.Get<int>("logicalId");
			var value = threadContext.Get<string>(ItemKey);

			var message = string.Format("[{0:00}] [{1:00}] Action, value: {2}", Thread.CurrentThread.ManagedThreadId, logicalId, value);
			Console.WriteLine(message);

			action(value);
		}

		[TestMethod]
		public void Fiddeling_With_HostExecutionContextManager()
		{
			// http://stackoverflow.com/questions/6939037/log4net-logicalthreadcontext-not-working
			// https://issues.apache.org/jira/browse/LOG4NET-317
			// http://logging.markmail.org/thread/q3bogptirf32g77r#query:+page:1+mid:tpgfykgizkpb2mxs+state:results
			// http://blogs.msdn.com/b/ericlippert/archive/2007/12/04/immutability-in-c-part-two-a-simple-immutable-stack.aspx
			var manager = new HostExecutionContextManager();

			_context.Set<string>("Item", "whatever");
			var context = manager.Capture();
		}
	}
}
