using CommunityToolkit.Mvvm.ComponentModel;

namespace ScadaGateway.UI.ViewModels.Dialogs
{
    public partial class ModbusChannelDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string channelName = "ModbusMaster1";
        [ObservableProperty] private string modbusType = "RTU"; // RTU or TCP
        [ObservableProperty] private string connectionType = "Serial"; // Serial or TCP
        [ObservableProperty] private bool enabled = true;

        // Serial
        [ObservableProperty] private string comPort = "COM1";
        [ObservableProperty] private int baudRate = 9600;
        [ObservableProperty] private int dataBits = 8;
        [ObservableProperty] private string stopBits = "One";
        [ObservableProperty] private string parity = "None";
        [ObservableProperty] private string handshake = "None";

        // TCP
        [ObservableProperty] private string serverAddress = "127.0.0.1";
        [ObservableProperty] private int clientPort = 502;
        [ObservableProperty] private int serverPort = 502;

        partial void OnConnectionTypeChanged(string value)
        {
            OnPropertyChanged(nameof(IsSerial));
            OnPropertyChanged(nameof(IsTcp));
        }

        public bool IsSerial => ConnectionType == "Serial";
        public bool IsTcp => ConnectionType == "TCP";
    }
}
