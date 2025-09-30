using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class DataTypeGroup
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = ""; // Ví dụ: T1, T2, T3, T4
        public string Function { get; set; } = ""; // Coil, Discrete, InputRegister, HoldingRegister
        public List<Point> Points { get; } = new();
    }
}
