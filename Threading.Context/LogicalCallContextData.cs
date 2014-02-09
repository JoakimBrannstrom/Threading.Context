using System;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;

namespace Threading.Context
{
	[Serializable]
	public class LogicalCallContextData<T> : ILogicalThreadAffinative where T : new()
	{
		T _data;

		public T Data { get { return _data; } }

		public LogicalCallContextData(T data)
		{
			if (!typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T))))
				throw new InvalidOperationException(typeof(T).Name + " is not serializable");

			if (data == null)
				throw new ArgumentNullException("data");

			_data = data;
		}

		public override string ToString()
		{
			return _data.ToString();
		}
	}
}
