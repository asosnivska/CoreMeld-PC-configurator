using CoreMeld.BLL.Services;
using Microsoft.Extensions.DependencyInjection;
namespace CoreMeld.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddTransient<IConfiguratorEngine, ConfiguratorEngine>();
        return services;
    }
}