using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Collections;

namespace Threading.Context.UnitTests
{
	[TestClass]
	public class LogicalThreadContextTests_WithHttpContextPresent : LogicalThreadContextTests
	{
		Mock<LogicalThreadContext> ctx;
		Dictionary<string, object> _requestCache = new Dictionary<string, object>();

		protected override IThreadContext ThreadContext { get { return ctx.Object; } }

		[TestInitialize]
		public void Setup()
		{
			ctx = new Mock<LogicalThreadContext>();
			ctx.CallBase = true;
			ctx
				.Protected()
				.Setup<IDictionary>("GetHttpContextItems")
				.Returns(() => _requestCache);
		}

		[TestMethod]
		public void GivenHttpRequestItemsExist_WhenStoringContextItems_ThenRequestItemsShouldBeUsed()
		{
			// Arrange

			// Act
			ThreadContext.Set(ItemKey, Guid.NewGuid().ToString());

			// Assert
			Assert.AreEqual(1, _requestCache.Count);
		}

		[TestMethod]
		public void GivenHttpRequestItemsExist_WhenFetchingContextItems_ThenRequestItemsShouldBeUsed()
		{
			// Arrange
			var expectedValue = Guid.NewGuid().ToString();
			_requestCache[ItemKey] = expectedValue;

			// Act
			var result = ThreadContext.Get<string>(ItemKey);

			// Assert
			Assert.AreEqual(expectedValue, result);
		}
	}

	[TestClass]
	public class LogicalThreadContextTests
	{
		protected const string ItemKey = "Item";
		protected virtual IThreadContext ThreadContext { get { return new LogicalThreadContext(); } }

		[TestCleanup]
		public void Cleanup()
		{
			ThreadContext.Remove(ItemKey);
		}

		[TestMethod]
		public void GivenNoInitialState_WhenItemIsFetched_ThenNullShouldBeReturned()
		{
			// Arrange

			// Act
			var item = ThreadContext.Get<string>(ItemKey);

			// Assert
			Assert.IsNull(item, "Scenario: " + GetType());
		}

		[TestMethod]
		public void GivenItemIsStored_WhenItemIsFetched_ThenStoredItemShouldBeReturned()
		{
			// Arrange
			var expectedItem = "Item is stored";
			ThreadContext.Set(ItemKey, expectedItem);

			// Act
			var item = ThreadContext.Get<string>(ItemKey);

			// Assert
			Assert.AreEqual(expectedItem, item, "Scenario: " + GetType());
		}

		[TestMethod]
		public void GivenItemIsStored_WhenItemIsFetchedFromThreadPoolThread_ThenStoredItemShouldBeReturned()
		{
			// Arrange
			var expectedItem = "Item is stored";
			ThreadContext.Set(ItemKey, expectedItem);
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
			Assert.AreEqual(expectedItem, item, "Scenario: " + GetType());
		}

		static void RunActionInBackground(object state)
		{
			WriteThreadInfo();

			var action = state as Action<string>;
			var threadContext = new LogicalThreadContext();
			var context = threadContext.Get<string>(ItemKey);

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
