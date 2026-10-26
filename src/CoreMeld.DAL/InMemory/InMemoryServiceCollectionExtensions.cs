using CoreMeld.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMeld.DAL.InMemory;

public static class InMemoryServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IComponentRepository, InMemoryComponentRepository>();
        services.AddSingleton<ICpuRepository, InMemoryCpuRepository>();
        services.AddSingleton<IMotherboardRepository, InMemoryMotherboardRepository>();
        services.AddSingleton<IRamRepository, InMemoryRamRepository>();
        services.AddSingleton<IGpuRepository, InMemoryGpuRepository>();
        services.AddSingleton<IStorageRepository, InMemoryStorageRepository>();
        services.AddSingleton<ICpuCoolerRepository, InMemoryCpuCoolerRepository>();
        services.AddSingleton<IPCCaseRepository, InMemoryPCCaseRepository>();
        services.AddSingleton<IPowerSupplyRepository, InMemoryPowerSupplyRepository>();
        return services;
    }
}