using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class Device
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<Point> Points { get; } = new();
    }
}
