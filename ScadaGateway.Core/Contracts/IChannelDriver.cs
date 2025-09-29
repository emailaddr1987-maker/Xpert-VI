using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ScadaGateway.Core.Models;

namespace ScadaGateway.Core.Contracts
{
    /// <summary>
    /// Contract that each driver/plugin must implement.
    /// </summary>
    public interface IChannelDriver
    {
        /// <summary> Unique protocol name, e.g. "ModbusMaster", "IEC104Slave", "Mock" </summary>
        string Protocol { get; }

        /// <summary> Create a channel model instance from id/name/config (no background work here). </summary>
        Channel CreateChannel(string id, string name, IDictionary<string, string>? config = null);

        /// <summary> Start driver activities for the channel (polling/listening). Should return when background tasks started. </summary>
        Task StartAsync(Channel channel, CancellationToken ct);

        /// <summary> Stop driver background activities for the channel. </summary>
        Task StopAsync(Channel channel);

        /// <summary> Optional: discover points (if driver supports discovery) </summary>
        Task<IEnumerable<Point>> DiscoverPointsAsync(Channel channel, CancellationToken ct);

        /// <summary> Optional synchronous read/write helpers drivers can implement (useful for write mapping). </summary>
        Task<object?> ReadPointAsync(Channel channel, Point point, CancellationToken ct);
        Task<bool> WritePointAsync(Channel channel, Point point, object? value, CancellationToken ct);
    }
}
