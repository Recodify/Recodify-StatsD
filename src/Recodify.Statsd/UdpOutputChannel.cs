using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Recodify.Statsd
{
	public class UdpOutputChannel : IChannel
	{
		private UdpClient _udpClient;

		public Socket ClientSocket => this._udpClient.Client;

		public UdpOutputChannel(string hostOrIPAddress, int port)
		{
			IPAddress address;
			if (!IPAddress.TryParse(hostOrIPAddress, out address))
			{
				address = Enumerable.First<IPAddress>(Dns.GetHostAddresses(hostOrIPAddress), p => p.AddressFamily == AddressFamily.InterNetwork);
			}

			this._udpClient = new UdpClient();
			this._udpClient.Connect(address, port);
		}

		public void Send(string line)
		{
			var bytes = Encoding.UTF8.GetBytes(line);
			this._udpClient.Send(bytes, bytes.Length);
		}

		public void Close()
		{
			_udpClient.Close();
		}
	}
}
