using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    
    public class Channel
    {
        //public string Id { get; set; } = "";
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string Protocol { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public List<Device> Devices { get; } = new();
        public Dictionary<string, string> Config { get; } = new();
        public List<Point> StatusPoints { get; } = new();
        public Channel()
        {
            // Khởi tạo các status points mặc định
            StatusPoints.Add(new Point
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Connected",
                DataType = PointDataType.Bool
            });
            StatusPoints.Add(new Point
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "DisconnectCounter",
                DataType = PointDataType.UInt32
            });

            // Giá trị ban đầu
            SetConnectionState(false);
        }

        public void SetConnectionState(bool connected)
        {
            var connectedPt = StatusPoints.Find(p => p.Name == "Connected");
            connectedPt?.SetValue(connected, "Good");

            if (!connected)
            {
                var counterPt = StatusPoints.Find(p => p.Name == "DisconnectCounter");
                if (counterPt != null)
                {
                    var current = counterPt.Value is uint v ? v : 0u;
                    counterPt.SetValue(current + 1, "Good");
                }
            }
        }
    

    }
}
