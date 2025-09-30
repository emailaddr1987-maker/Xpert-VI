using System.Windows;

namespace ScadaGateway.UI.Views
{
    public partial class GatewaySettingDialog : Window
    {
        public GatewaySettingDialog()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
