using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ScadaGateway.Core.Contracts;
using ScadaGateway.Core.Models;

namespace ScadaGateway.Drivers.Mock
{
    public class MockDriver : IChannelDriver
    {
        public string Protocol => "Mock";

        public Channel CreateChannel(string id, string name, IDictionary<string, string>? config = null)
        {
            var ch = new Channel { Id = id, Name = name, Protocol = Protocol };
            var dev = new Device { Id = $"{id}.dev1", Name = "MockDevice" };
            dev.Points.Add(new Point { Id = $"{id}.dev1.p1", Name = "RandomBool", DataType = PointDataType.Bool });
            dev.Points.Add(new Point { Id = $"{id}.dev1.p2", Name = "RandomFloat", DataType = PointDataType.Single });
            ch.Devices.Add(dev);
            return ch;
        }

        private CancellationTokenSource? _cts;
        public async Task StartAsync(Channel channel, CancellationToken ct)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var rnd = new Random();
            await Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    foreach (var dev in channel.Devices)
                    {
                        foreach (var p in dev.Points)
                        {
                            if (p.DataType == PointDataType.Bool) p.SetValue(rnd.Next(0, 2) == 1);
                            else if (p.DataType == PointDataType.Single) p.SetValue((float)(rnd.NextDouble() * 100.0));
                            else p.SetValue(null, "Bad");
                        }
                    }
                    await Task.Delay(1000, _cts.Token);
                }
            }, _cts.Token);
        }

        public Task StopAsync(Channel channel)
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }
    }
}
