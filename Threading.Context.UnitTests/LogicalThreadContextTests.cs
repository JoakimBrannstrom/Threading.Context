using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Threading.Context.UnitTests
{
	[TestClass]
	public class LogicalThreadContextTests
	{
		const string ItemKey = "Item";
		static IThreadContext _context { get { return new LogicalThreadContext(); } }

		[TestCleanup]
		public void Cleanup()
		{
			_context.Remove(ItemKey);
		}

		[TestMethod]
		public void GivenNoInitialState_WhenItemIsFetched_ThenNullShouldBeReturned()
		{
			// Arrange

			// Act
			var item = _context.Get<string>(ItemKey);

			// Assert
			Assert.IsNull(item);
		}

		[TestMethod]
		public void GivenItemIsStored_WhenItemIsFetched_ThenStoredItemShouldBeReturned()
		{
			// Arrange
			var expectedItem = "Item is stored";
			_context.Set<string>(ItemKey, expectedItem);

			// Act
			var item = _context.Get<string>(ItemKey);

			// Assert
			Assert.AreEqual(expectedItem, item);
		}

		[TestMethod]
		public void GivenItemIsStored_WhenItemIsFetchedFromThreadPoolThread_ThenStoredItemShouldBeReturned()
		{
			// Arrange
			var expectedItem = "Item is stored";
			_context.Set<string>(ItemKey, expectedItem);
			string item = null;
			Action<string> state = (string value) => item = value;

			WriteThreadInfo();

			// Act
			ThreadPool.QueueUserWorkItem(new WaitCallback(RunActionInBackground), state);

			var retries = 0;
			while(retries < 100 && string.IsNullOrEmpty(item))
			{
				retries++;
				Thread.Sleep(10);
			}

			Console.WriteLine("retries: " + retries);

			// Assert
			Assert.AreEqual(expectedItem, item);
		}

		static void RunActionInBackground(object state)
		{
			WriteThreadInfo();

			var action = state as Action<string>;
			var context = _context.Get<string>(ItemKey);
			action(context);
		}

		private static void WriteThreadInfo()
		{
			Console.WriteLine("**********************************");
			Console.WriteLine("ManagedThreadId: " + Thread.CurrentThread.ManagedThreadId);
			Console.WriteLine("IsThreadPoolThread: " + Thread.CurrentThread.IsThreadPoolThread);
			Console.WriteLine("IsBackground: " + Thread.CurrentThread.IsBackground);
			Console.WriteLine("");
		}
	}
}
