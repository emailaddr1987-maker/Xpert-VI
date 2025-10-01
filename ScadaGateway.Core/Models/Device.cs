using System.Collections.Generic;

namespace ScadaGateway.Core.Models
{
    public class Device
    {
        //public string Id { get; set; } = "";
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
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
        public List<Point> StatusPoints { get; } = new();

        public Device()
        {
            // Khởi tạo status points mặc định
            StatusPoints.Add(new Point
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Online",
                DataType = PointDataType.Bool
            });
            StatusPoints.Add(new Point
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "OfflineCounter",
                DataType = PointDataType.UInt32
            });

            // Giá trị ban đầu
            SetOnlineState(false);
        }

        public void SetOnlineState(bool online)
        {
            var onlinePt = StatusPoints.Find(p => p.Name == "Online");
            onlinePt?.SetValue(online, "Good");

            if (!online)
            {
                var counterPt = StatusPoints.Find(p => p.Name == "OfflineCounter");
                if (counterPt != null)
                {
                    var current = counterPt.Value is uint v ? v : 0u;
                    counterPt.SetValue(current + 1, "Good");
                }
            }
        }
    }
}
