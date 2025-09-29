using CommunityToolkit.Mvvm.ComponentModel;
using ScadaGateway.Core.Models;
using System;
using System.Windows.Threading;

namespace ScadaGateway.UI.ViewModels
{
    public partial class PointViewModel : ObservableObject
    {
        private readonly Point _model;

        public string Id => _model.Id;
        [ObservableProperty] private string name;
        [ObservableProperty] private string type;
        [ObservableProperty] private object? value;
        [ObservableProperty] private string quality;
        [ObservableProperty] private DateTime timestamp;

        public Point Model => _model;

        public PointViewModel(Point model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            name = _model.Name;
            type = _model.DataType.ToString();
            value = _model.Value;
            quality = _model.Quality;
            timestamp = _model.Timestamp.ToLocalTime();

            // subscribe để cập nhật realtime khi driver thay đổi Point
            _model.ValueChanged += (s, p) =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                dispatcher.Invoke(() =>
                {
                    Value = p.Value;
                    Quality = p.Quality;
                    Timestamp = p.Timestamp.ToLocalTime();
                });
            };
        }
    }
}
