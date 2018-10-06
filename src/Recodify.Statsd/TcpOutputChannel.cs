using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Recodify.Statsd
{
	internal sealed class TcpOutputChannel : IChannel
	{
		private TcpClient _tcpClient;
		private NetworkStream _stream;
		private readonly object _reconnectLock;
		private readonly string _host;
		private readonly int _port;
		private readonly bool _reconnectEnabled;
		private readonly int _retryAttempts;

		public TcpOutputChannel(string host, int port, bool reconnectEnabled = true, int retryAttempts = 3)
		{
			this._host = host;
			this._port = port;
			this._reconnectEnabled = reconnectEnabled;
			this._retryAttempts = retryAttempts;
			this._tcpClient = new TcpClient();
			this._reconnectLock = new object();
		}

		public void Send(string line)
		{
			this.SendWithRetry(line, this._reconnectEnabled ? this._retryAttempts - 1 : 0);
		}

		public void Close()
		{
			_tcpClient.Close();
		}

		private void SendWithRetry(string line, int attemptsLeft)
		{
			try
			{
				if (!this._tcpClient.Connected)
				{
					this.RestoreConnection();
				}

				var bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
				this._stream.Write(bytes, 0, bytes.Length);
			}
			catch (IOException ex)
			{
				if (attemptsLeft > 0)
				{
					this.SendWithRetry(line, --attemptsLeft);
				}
				else
				{
					Trace.TraceWarning("Sending metrics via TCP failed with an IOException: {0}", new object[1]
		  {
			 ex.Message
		  });
				}
			}
			catch (SocketException ex)
			{
				if (attemptsLeft > 0)
				{
					this.SendWithRetry(line, --attemptsLeft);
				}
				else
				{
					Trace.TraceWarning("Sending metrics via TCP failed with a SocketException: {0}, code: {1}", ex.Message, ex.SocketErrorCode.ToString());
				}
			}
		}

		private void RestoreConnection()
		{
			lock (this._reconnectLock)
			{
				if (this._tcpClient.Connected)
				{
					return;
				}

				this._tcpClient.Connect(this._host, this._port);
				this._stream = this._tcpClient.GetStream();
			}
		}
	}
}
