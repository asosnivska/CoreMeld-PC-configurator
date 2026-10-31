using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings;
using Microsoft.Extensions.DependencyInjection;
namespace CoreMeld.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddTransient<IConfiguratorEngine, ConfiguratorEngine>();
        services.AddSingleton<IAllocationSettingsProvider>(_ =>
            new JsonAllocationSettingsProvider(
                Path.Combine(AppContext.BaseDirectory, "Settings", "allocation-weights.json")));
        services.AddTransient<IBudgetCalculator, BudgetCalculator>();
        return services;
    }
}