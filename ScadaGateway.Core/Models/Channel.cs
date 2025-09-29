using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class Channel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Protocol { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public List<Device> Devices { get; } = new();
        public Dictionary<string, string> Config { get; } = new();
    }
}
