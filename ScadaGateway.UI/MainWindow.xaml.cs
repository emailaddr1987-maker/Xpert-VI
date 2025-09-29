using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScadaGateway.UI.ViewModels;

namespace ScadaGateway.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OnSelectedTreeItemChanged(e.NewValue);
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem tvi) tvi.IsSelected = true;
        }
    }
}
