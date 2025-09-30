using System.Reflection;
using System.Windows;

namespace ScadaGateway.UI.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            AppNameText.Text = "XpertGateway";
            VersionText.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";
        }
    }
}
