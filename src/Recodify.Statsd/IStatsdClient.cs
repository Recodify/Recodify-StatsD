using StatsdClient;

namespace Recodify.Statsd
{
	public interface IStatsdClient : IStatsd
	{
		void Close();
	}
}
