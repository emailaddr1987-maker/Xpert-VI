using System.Collections.Generic;
using System.Windows;
using ScadaGateway.Core.Models;
using ScadaGateway.UI.ViewModels.Dialogs;

namespace ScadaGateway.UI.Dialogs
{
    public partial class ModbusChannelDialog : Window, IChannelConfigDialog
    {
        private readonly ModbusChannelDialogViewModel _vm;

        // Constructor cho Add mới
        public ModbusChannelDialog()
        {
            InitializeComponent();
            _vm = new ModbusChannelDialogViewModel();
            DataContext = _vm;
        }

        // Constructor cho Edit channel có sẵn
        public ModbusChannelDialog(Channel existing)
        {
            InitializeComponent();
            _vm = new ModbusChannelDialogViewModel
            {
                ChannelName = existing.Name,
                Enabled = existing.Enabled,
                ModbusType = existing.Config.TryGetValue("ModbusType", out var modbusType) ? modbusType : "RTU",
                ConnectionType = existing.Config.TryGetValue("ConnectionType", out var connType) ? connType : "Serial",
                ComPort = existing.Config.TryGetValue("ComPort", out var comPort) ? comPort : "COM1",
                BaudRate = existing.Config.TryGetValue("BaudRate", out var baud) && int.TryParse(baud, out var b) ? b : 9600,
                DataBits = existing.Config.TryGetValue("DataBits", out var db) && int.TryParse(db, out var dbits) ? dbits : 8,
                StopBits = existing.Config.TryGetValue("StopBits", out var sb) ? sb : "One",
                Parity = existing.Config.TryGetValue("Parity", out var par) ? par : "None",
                Handshake = existing.Config.TryGetValue("Handshake", out var hs) ? hs : "None",
                ServerAddress = existing.Config.TryGetValue("ServerAddress", out var addr) ? addr : "127.0.0.1",
                ClientPort = existing.Config.TryGetValue("ClientPort", out var cport) && int.TryParse(cport, out var cp) ? cp : 502,
                ServerPort = existing.Config.TryGetValue("ServerPort", out var sport) && int.TryParse(sport, out var sp) ? sp : 502,
            };
            DataContext = _vm;
        }

        public string ChannelName => _vm.ChannelName;
        public string Protocol => "ModbusMaster";
        public bool Enabled => _vm.Enabled;

        // Xuất config ra dictionary để lưu vào Channel
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
