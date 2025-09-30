using ScadaGateway.Core.Models;
using System.Collections.ObjectModel;

namespace ScadaGateway.UI.ViewModels
{
    public class DataTypeGroupViewModel
    {
        public DataTypeGroup Model { get; }
        public string Name => Model.Name;
        public string Function => Model.Function;

        public ObservableCollection<PointViewModel> Points { get; } = new();

        public DataTypeGroupViewModel(DataTypeGroup model)
        {
            Model = model;
            foreach (var p in model.Points)
                Points.Add(new PointViewModel(p));
        }
    }
}
