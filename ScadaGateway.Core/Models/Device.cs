using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class Device
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";

        // Danh sách DataType Groups
        public List<DataTypeGroup> DataTypeGroups { get; } = new();

        public List<Point> Points { get; } = new();
    }
}
