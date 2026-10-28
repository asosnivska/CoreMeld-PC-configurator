using CoreMeld.BLL;
using CoreMeld.DAL;
using CoreMeld.DAL.Repositories;
using CoreMeld.UI.Desktop.ViewModels;
using CoreMeld.UI.Desktop.Views;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using CoreMeld.DAL.InMemory;

namespace CoreMeld.UI.Desktop;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;
    private readonly bool _useFakeData;

    public App()
    {
        var services = new ServiceCollection();

        var envPath = FindEnvFile();
        if (envPath != null)
            Env.Load(envPath);

        services.AddBusinessLogic();

        _useFakeData = !string.Equals(Environment.GetEnvironmentVariable("USE_FAKE_DATA"), "false", StringComparison.OrdinalIgnoreCase);
        if (_useFakeData)
        {
            services.AddInMemoryRepositories();
        }

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
#if DEBUG
        if (!_useFakeData)
            await CheckDatabaseAsync();
#endif
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

#if DEBUG
    private static async Task CheckDatabaseAsync()
    {
        try
        {
            var cs = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(cs))
            {
                MessageBox.Show("DB_CONNECTION_STRING не задано (.env)");
                return;
            }

            var repo = new ComponentRepository(new DbConnectionManager(cs));
            var components = (await repo.GetAllAsync()).ToList();

            MessageBox.Show(
                $"Підключення до БД успішне.\nДеталей в базі: {components.Count}\n\n" +
                string.Join("\n", components.Select(c => $"{c.Name} : {c.Price} грн")));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Помилка БД:\n" + ex.Message);
        }
    }
#endif

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }

    private static string? FindEnvFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }
}