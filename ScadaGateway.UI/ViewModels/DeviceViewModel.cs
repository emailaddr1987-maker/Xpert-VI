using ScadaGateway.Core.Models;
using System.Collections.ObjectModel;

namespace ScadaGateway.UI.ViewModels
{
    public class DeviceViewModel
    {
        public Device Model { get; }
        public string Name => Model.Name;

        public ObservableCollection<DataTypeGroupViewModel> DataTypeGroups { get; } = new();

        public DeviceViewModel(Device model)
        {
            Model = model;
            foreach (var g in model.DataTypeGroups)
                DataTypeGroups.Add(new DataTypeGroupViewModel(g));
        }
    }
}
