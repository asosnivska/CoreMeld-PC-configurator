using System.Windows;
using CoreMeld.UI.Desktop.ViewModels;


namespace CoreMeld.UI.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
