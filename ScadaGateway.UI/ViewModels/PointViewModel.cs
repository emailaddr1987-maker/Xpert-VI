using CommunityToolkit.Mvvm.ComponentModel;
using ScadaGateway.Core.Models;

namespace ScadaGateway.UI.ViewModels
{
    public partial class PointViewModel : ObservableObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public object? Value { get; set; }
        public string Quality { get; set; } = "Bad";
        public DateTime Timestamp { get; set; }

        public PointViewModel() { }

        public PointViewModel(Point model)
        {
            Id = model.Id;
            Name = model.Name;
            Type = model.DataType.ToString();
            Value = model.Value;
            Quality = model.Quality;
            Timestamp = model.Timestamp;
        }
    }
}
