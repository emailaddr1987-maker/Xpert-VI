using System;

namespace ScadaGateway.Core.Models
{
    public enum PointDataType { Bool, Int8, Int16, Int32, UInt16, UInt32, Single, Double, String }

    public class Point
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public PointDataType DataType { get; set; }
        public object? Value { get; private set; }
        public string Quality { get; private set; } = "Bad";
        public DateTime Timestamp { get; private set; } = DateTime.MinValue;
        public System.Collections.Generic.Dictionary<string, string> Meta { get; set; } = new();

        public event EventHandler<Point>? ValueChanged;

        public void SetValue(object? value, string quality = "Good")
        {
            Value = value;
            Quality = quality;
            Timestamp = DateTime.UtcNow;
            ValueChanged?.Invoke(this, this);
        }
    }
}
