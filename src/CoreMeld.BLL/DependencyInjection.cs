using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings; 
using CoreMeld.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMeld.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(
        this IServiceCollection services,
        bool? useFakeData = null,
        string? connectionString = null)
    {
        services.AddDataAccess(useFakeData, connectionString);
        services.AddTransient<IConfiguratorEngine, ConfiguratorEngine>();
        services.AddSingleton<IAllocationSettingsProvider>(_ =>
            new JsonAllocationSettingsProvider(
                Path.Combine(AppContext.BaseDirectory, "Settings", "allocation-weights.json")
                ));
        services.AddTransient<IBudgetCalculator, BudgetCalculator>();

        services.AddSingleton<IFormFactorSpecsProvider>(_ =>
            new JsonFormFactorSpecsProvider(
                Path.Combine(AppContext.BaseDirectory, "Settings", "form-factors.json")
            ));

        services.AddTransient<ISpaceControlService, SpaceControlService>();
        services.AddTransient<ITdpCalculator, TdpCalculator>();
        services.AddTransient<ICompatibilityChecker, CompatibilityChecker>();
        return services;
    }
}
