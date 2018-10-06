Recodify StatsD
=================

Simple c# StatsD wrapper

Usage
---------------

			var statsd = new Statsd("127.0.0.1", 8181);
			var statsdUdp = new Statsd("127.0.0.1", 8181, ConnectionType.Udp, prefix: "myapp-", rethrowOnError: true, retryOnDisconnect: true, retryAttempts: 10);
			var statsdTcp = new Statsd("127.0.0.1", 8181, ConnectionType.Tcp, prefix: "myapp-", rethrowOnError: true, retryOnDisconnect: true, retryAttempts: 10);

			statsd.LogCount("hits", 1);
			statsd.LogGauge("speed", 100);
			statsd.LogTiming("response", 260);
			statsd.LogRaw("rawdata", 100);
			statsd.LogCalendargram("date", 25, "day");
