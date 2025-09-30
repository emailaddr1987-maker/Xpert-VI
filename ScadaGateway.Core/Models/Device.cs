using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class Device
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public int Timeout { get; set; } = 3000;

        // Request intervals (ms)
        public Dictionary<string, int> RequestIntervals { get; set; } = new();

        // Max number each request
        public Dictionary<string, int> MaxNumbers { get; set; } = new();
        // Danh sách DataType Groups
        public List<DataTypeGroup> DataTypeGroups { get; } = new();

        public List<Point> Points { get; } = new();
    }
}
