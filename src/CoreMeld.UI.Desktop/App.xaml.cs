using System.Windows;
using CoreMeld.BLL;
using CoreMeld.UI.Desktop.ViewModels;
using CoreMeld.UI.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMeld.UI.Desktop;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();

        services.AddBusinessLogic();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}