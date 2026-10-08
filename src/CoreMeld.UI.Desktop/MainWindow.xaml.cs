using System.Windows;
using CoreMeld.BLL.Services;

namespace CoreMeld.UI.Desktop;

public partial class MainWindow : Window
{
    private readonly IConfiguratorEngine _engine;

    public MainWindow(IConfiguratorEngine engine)
    {
        InitializeComponent();
        _engine = engine;
    }
}