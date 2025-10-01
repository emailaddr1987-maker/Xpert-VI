using System.Windows;
using System.Xml.Linq;
using ScadaGateway.Core.Models;

namespace ScadaGateway.UI.Views
{
    public partial class AddDeviceWindow : Window
    {
        // Cho phép MainViewModel gán và đọc lại Device
        public Device Device { get; set; }

        public AddDeviceWindow()
        {
            InitializeComponent();
            Device = new Device();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // cập nhật lại Device từ UI
            Device.Name = txtName.Text;
            Device.Id = txtId.Text;
            Device.Enabled = chkEnabled.IsChecked ?? false;
            Device.Timeout = int.TryParse(txtTimeout.Text, out var t) ? t : 3000;

            Device.RequestIntervals["Coil"] = int.Parse(txtIntervalCoil.Text);
            Device.RequestIntervals["Discrete"] = int.Parse(txtIntervalDiscrete.Text);
            Device.RequestIntervals["HoldingRegister"] = int.Parse(txtIntervalHolding.Text);
            Device.RequestIntervals["InputRegister"] = int.Parse(txtIntervalInput.Text);

            Device.MaxNumbers["Coil"] = int.Parse(txtMaxCoil.Text);
            Device.MaxNumbers["Discrete"] = int.Parse(txtMaxDiscrete.Text);
            Device.MaxNumbers["HoldingRegister"] = int.Parse(txtMaxHolding.Text);
            Device.MaxNumbers["InputRegister"] = int.Parse(txtMaxInput.Text);

            DialogResult = true;
        }
    }
}
