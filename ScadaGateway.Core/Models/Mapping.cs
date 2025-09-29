using System;

namespace ScadaGateway.Core.Models
{
    public class Mapping
    {
        public string Id { get; set; } = "";
        public string SourcePointId { get; set; } = "";
        public string DestinationPointId { get; set; } = "";
        public bool TwoWay { get; set; } = false;
        public TimeSpan? Interval { get; set; } // null => upon-change
        public string? Expression { get; set; } // optional transform
    }
}
