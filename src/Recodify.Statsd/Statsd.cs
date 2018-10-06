using StatsdClient;
using System;
using System.Diagnostics;

namespace Recodify.Statsd
{
	public class Statsd : IStatsdClient
	{
		private string _prefix;
		private IChannel _outputChannel;

		public Statsd(string host, int port)
		{
			if (string.IsNullOrEmpty(host))
			{
				Trace.TraceWarning("Statsd client initialised with empty host address. Dropping back to NullOutputChannel.");
				this.InitialiseInternal(() => new NullOutputChannel(), "", false);
			}
			else
			{
				this.InitialiseInternal(() => new UdpOutputChannel(host, port), "", false);
			}
		}

		public Statsd(string host, int port, ConnectionType connectionType = ConnectionType.Udp, string prefix = null, bool rethrowOnError = false, bool retryOnDisconnect = true, int retryAttempts = 3)
		{
			this.InitialiseInternal((Func<IChannel>)(() =>
			{
				if (connectionType != ConnectionType.Tcp)
				{
					return new UdpOutputChannel(host, port);
				}
				else
				{
					return new TcpOutputChannel(host, port, retryOnDisconnect, retryAttempts);
				}
			}), prefix, rethrowOnError);
		}

		public Statsd(string host, int port, string prefix = null, bool rethrowOnError = false, IChannel outputChannel = null)
		{
			if (outputChannel == null)
			{
				this.InitialiseInternal(() => new UdpOutputChannel(host, port), prefix, rethrowOnError);
			}
			else
			{
				this.InitialiseInternal(() => outputChannel, prefix, rethrowOnError);
			}
		}

		private void InitialiseInternal(Func<IChannel> createOutputChannel, string prefix, bool rethrowOnError)
		{
			this._prefix = prefix;
			if (this._prefix != null)
			{
				if (this._prefix.EndsWith("."))
				{
					this._prefix = this._prefix.Substring(0, this._prefix.Length - 1);
				}
			}
			try
			{
				this._outputChannel = createOutputChannel();
			}
			catch (Exception ex)
			{
				if (rethrowOnError)
				{
					throw;
				}
				else
				{
					Trace.TraceError("Could not initialise the Statsd client: {0} - falling back to NullOutputChannel.", new object[1]
					  {
						 ex.Message
					  });
					this._outputChannel = new NullOutputChannel();
				}
			}
		}

		public void LogCount(string name, int count = 1)
		{
			this.SendMetric("c", name, this._prefix, count, null);
		}

		public void LogTiming(string name, int milliseconds)
		{
			this.SendMetric("ms", name, this._prefix, milliseconds, null);
		}

		public void LogTiming(string name, long milliseconds)
		{
			this.LogTiming(name, (int)milliseconds);
		}

		public void LogGauge(string name, int value)
		{
			this.SendMetric("g", name, this._prefix, value, null);
		}

		public void LogSet(string name, int value)
		{
			this.SendMetric("s", name, this._prefix, value, null);
		}

		public void LogCalendargram(string name, string value, string period)
		{
			this.SendMetric("cg", name, this._prefix, value, period);
		}

		public void LogCalendargram(string name, int value, string period)
		{
			this.SendMetric("cg", name, this._prefix, value, period);
		}

		public void LogRaw(string name, int value, long? epoch = null)
		{
			this.SendMetric("r", name, string.Empty, value, epoch.HasValue ? epoch.ToString() : null);
		}

		public void Close()
		{
			_outputChannel.Close();
		}

		private void SendMetric(string metricType, string name, string prefix, int value, string postFix = null)
		{
			if (value < 0)
			{
				Trace.TraceWarning(string.Format("Metric value for {0} was less than zero: {1}. Not sending.", name, value));
			}
			else
			{
				this.SendMetric(metricType, name, prefix, value.ToString(), postFix);
			}
		}

		private void SendMetric(string metricType, string name, string prefix, string value, string postFix = null)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			this._outputChannel.Send(this.PrepareMetric(metricType, name, prefix, value, postFix));
		}

		protected virtual string PrepareMetric(string metricType, string name, string prefix, string value, string postFix = null)
		{
			return (string.IsNullOrEmpty(prefix) ? name : prefix + "." + name) + ":" + value + "|" + metricType + (postFix == null ? string.Empty : "|" + postFix);
		}
	}
}
