using System.IO;
using System.Windows;

namespace ScadaGateway.UI.Views
{
    public partial class LicenceWindow : Window
    {
        public LicenceWindow()
        {
            InitializeComponent();
            LoadLicence();
        }

        private void LoadLicence()
        {
            string path = "LICENCE.txt";
            if (File.Exists(path))
                LicenceTextBox.Text = File.ReadAllText(path);
            else
                LicenceTextBox.Text = "No licence file found.";
        }
    }
}
