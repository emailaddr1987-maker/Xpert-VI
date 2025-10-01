using ScadaGateway.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScadaGateway.UI.ViewModels
{
    public partial class DeviceViewModel : ObservableObject
    {
        public Device Model { get; }
        public string Name => Model.Name;

        public ObservableCollection<DataTypeGroupViewModel> DataTypeGroups { get; } = new();
        public ObservableCollection<PointViewModel> StatusPoints { get; } = new();

        public DeviceViewModel(Device model)
        {
            Model = model;

            foreach (var g in model.DataTypeGroups)
                DataTypeGroups.Add(new DataTypeGroupViewModel(g));

            foreach (var p in model.StatusPoints)
                StatusPoints.Add(new PointViewModel(p));
        }

        // thêm Refresh để sửa lỗi CS1061
        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DataTypeGroups));
            OnPropertyChanged(nameof(StatusPoints));
        }
    }
}
