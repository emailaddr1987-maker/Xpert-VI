using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ScadaGateway.Core.Models;

namespace ScadaGateway.Core.Services
{
    public class MappingEngine
    {
        private readonly ConcurrentDictionary<string, Mapping> _mappings = new();
        private readonly List<Timer> _timers = new();

        public void RegisterMapping(Mapping m, Func<string, Point?> lookupPoint, Action<Point, object?> writePoint)
        {
            _mappings[m.Id] = m;
            if (m.Interval.HasValue)
            {
                var timer = new Timer(_ =>
                {
                    var src = lookupPoint(m.SourcePointId);
                    var dst = lookupPoint(m.DestinationPointId);
                    if (src != null && dst != null)
                    {
                        // direct copy, expression transform can be implemented here
                        writePoint(dst, src.Value);
                    }
                }, null, TimeSpan.Zero, m.Interval.Value);
                _timers.Add(timer);
            }
        }

        // called by GatewayService when a point changes
        public void OnPointChanged(Point p, Func<string, Point?> lookupPoint, Action<Point, object?> writePoint)
        {
            var relevant = _mappings.Values.Where(mm => mm.Interval == null && mm.SourcePointId == p.Id);
            foreach (var m in relevant)
            {
                var dst = lookupPoint(m.DestinationPointId);
                if (dst != null) writePoint(dst, p.Value);
            }
        }
    }
}
