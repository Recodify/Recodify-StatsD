namespace Recodify.Statsd
{
	public class NullOutputChannel : IChannel
	{
		public void Send(string line)
		{
		}

		public void Close()
		{
		}
	}
}
