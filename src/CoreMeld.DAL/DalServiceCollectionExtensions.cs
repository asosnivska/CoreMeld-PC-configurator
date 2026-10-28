using CoreMeld.DAL.Repositories;
using CoreMeld.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMeld.DAL;

public static class DalServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseRepositories(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new DbConnectionManager(connectionString));
        services.AddScoped<IComponentRepository, ComponentRepository>();
        services.AddScoped<ICpuRepository, CpuRepository>();
        services.AddScoped<IMotherboardRepository, MotherboardRepository>();
        services.AddScoped<IGpuRepository, GpuRepository>();
        services.AddScoped<IRamRepository, RamRepository>();
        services.AddScoped<IStorageRepository, StorageRepository>();
        services.AddScoped<ICpuCoolerRepository, CpuCoolerRepository>();
        services.AddScoped<IPCCaseRepository, PCCaseRepository>();
        services.AddScoped<IPowerSupplyRepository, PowerSupplyRepository>();
        return services;
    }
}

