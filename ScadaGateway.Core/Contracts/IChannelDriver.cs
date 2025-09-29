using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ScadaGateway.Core.Models;

namespace ScadaGateway.Core.Contracts
{
    public interface IChannelDriver
    {
        string Protocol { get; } // e.g. "ModbusMaster", "IEC104Slave", "Internal", "Mock"
        Channel CreateChannel(string id, string name, IDictionary<string, string>? config = null);
        Task StartAsync(Channel channel, CancellationToken ct);
        Task StopAsync(Channel channel);
    }
}
