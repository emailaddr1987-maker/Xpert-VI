using System.Collections.Generic;
using System.Windows;
using ScadaGateway.UI.Dialogs;
using ScadaGateway.UI.ViewModels.Dialogs;

namespace ScadaGateway.UI.Dialogs
{
    public partial class ModbusChannelDialog : Window, IChannelConfigDialog
    {
        private readonly ModbusChannelDialogViewModel _vm;

        public ModbusChannelDialog()
        {
            InitializeComponent();
            _vm = new ModbusChannelDialogViewModel();
            DataContext = _vm;
        }

        public string ChannelName => _vm.ChannelName;
        public string Protocol => "ModbusMaster";
        public bool Enabled => _vm.Enabled;

        public Dictionary<string, string> GetConfig()
        {
            var cfg = new Dictionary<string, string>
            {
                ["ChannelName"] = _vm.ChannelName ?? "",
                ["ModbusType"] = _vm.ModbusType ?? "",
                ["ConnectionType"] = _vm.ConnectionType ?? "",
                ["ComPort"] = _vm.ComPort ?? "",
                ["BaudRate"] = _vm.BaudRate.ToString(),
                ["DataBits"] = _vm.DataBits.ToString(),
                ["StopBits"] = _vm.StopBits ?? "",
                ["Parity"] = _vm.Parity ?? "",
                ["Handshake"] = _vm.Handshake ?? "",
                ["ServerAddress"] = _vm.ServerAddress ?? "",
                ["ClientPort"] = _vm.ClientPort.ToString(),
                ["ServerPort"] = _vm.ServerPort.ToString(),
            };
            return cfg;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
