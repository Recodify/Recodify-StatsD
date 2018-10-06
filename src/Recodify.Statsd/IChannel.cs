using StatsdClient;

namespace Recodify.Statsd
{
	public interface IChannel : IOutputChannel
	{
		void Close();
	}
}
