using CommunityToolkit.Mvvm.ComponentModel;
using ScadaGateway.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace ScadaGateway.UI.ViewModels
{
    public partial class DeviceViewModel : ObservableObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";

        public ObservableCollection<PointViewModel> Points { get; } = new();

        public Device Model { get; }

        public DeviceViewModel(Device model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Id = model.Id;
            Name = model.Name;

            foreach (var p in model.Points)
                Points.Add(new PointViewModel(p));
        }
    }
}
