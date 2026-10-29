using CoreMeld.BLL.Services;
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

        services.AddTransient<IComponentService, ComponentService>();
        services.AddTransient<IPCBuildService, PCBuildService>();
        services.AddTransient<IConfiguratorEngine, ConfiguratorEngine>();

        return services;
    }
}
