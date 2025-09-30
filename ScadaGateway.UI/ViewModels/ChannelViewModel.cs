using CommunityToolkit.Mvvm.ComponentModel;
using ScadaGateway.Core.Models;
using System;
using System.Collections.ObjectModel;

namespace ScadaGateway.UI.ViewModels
{
    public partial class ChannelViewModel : ObservableObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Protocol { get; set; } = "";
        public bool Enabled { get; set; }

        // ✅ Bổ sung hiển thị trạng thái Enable/Disable
        public string DisplayName => $"{Name} ({Protocol}) {(Enabled ? "Enable" : "Disable")}";

        public ObservableCollection<DeviceViewModel> Devices { get; } = new();

        public Channel Model { get; }

        public ChannelViewModel(Channel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Id = model.Id;
            Name = model.Name;
            Protocol = model.Protocol;
            Enabled = model.Enabled;

            foreach (var d in model.Devices)
                Devices.Add(new DeviceViewModel(d));
        }

        public void RefreshDisplayName()
        {
            OnPropertyChanged(nameof(DisplayName));
        }
    }
}
