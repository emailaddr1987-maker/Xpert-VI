using ScadaGateway.Core.Contracts;
using ScadaGateway.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaGateway.Core.Services
{
    public class GatewayService
    {
        private readonly DriverManager _driverManager;
        private readonly MappingEngine _mappingEngine;
        private readonly List<(Channel ch, IChannelDriver drv, CancellationTokenSource cts, Task runner)> _running = new();

        public List<Channel> Channels { get; } = new();
        public List<Mapping> Mappings { get; } = new();

        public GatewayService(DriverManager driverManager, MappingEngine mappingEngine)
        {
            _driverManager = driverManager;
            _mappingEngine = mappingEngine;
        }

        public Channel CreateChannel(string protocol, string id, string name, IDictionary<string, string>? config = null)
        {
            var drv = _driverManager.GetDriver(protocol) ?? throw new InvalidOperationException($"Driver {protocol} not found");
            var ch = drv.CreateChannel(id, name, config);
            Channels.Add(ch);
            return ch;
        }

        public async Task StartChannelAsync(Channel ch)
        {
            var drv = _driverManager.GetDriver(ch.Protocol) ?? throw new InvalidOperationException($"Driver {ch.Protocol} not found");
            var cts = new CancellationTokenSource();
            // subscribe for mapping
            foreach (var dev in ch.Devices)
                foreach (var p in dev.Points)
                    p.ValueChanged += (s, pt) => _mappingEngine.OnPointChanged(pt, LookupPoint, WritePoint);

            // start driver
            var t = drv.StartAsync(ch, cts.Token);
            _running.Add((ch, drv, cts, t));
            await Task.CompletedTask;
        }

        public async Task StopAllAsync()
        {
            foreach (var r in _running)
            {
                try
                {
                    r.cts.Cancel();
                    await r.drv.StopAsync(r.ch);
                }
                catch { }
            }
            _running.Clear();
        }

        public Point? LookupPoint(string id) => Channels.SelectMany(c => c.Devices).SelectMany(d => d.Points).FirstOrDefault(p => p.Id == id);
        public void WritePoint(Point p, object? value) => p.SetValue(value);
    }
}
