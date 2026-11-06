using CoreMeld.DAL.InMemory;
using CoreMeld.DAL.Repositories;
using CoreMeld.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMeld.DAL;

public static class DalServiceCollectionExtensions
{
    /// <summary>
    /// Registers DAL services with a fake-data switch (UseFakeData / USE_FAKE_DATA).
    /// If true, registers InMemory repositories. If false, registers real PostgreSQL ADO.NET repositories.
    /// </summary>
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        bool? useFakeData = null,
        string? connectionString = null)
    {
        var fakeDataSetting = useFakeData ?? !string.Equals(
            Environment.GetEnvironmentVariable("USE_FAKE_DATA") ?? Environment.GetEnvironmentVariable("UseFakeData"),
            "false",
            StringComparison.OrdinalIgnoreCase);

        if (fakeDataSetting)
        {
            services.AddInMemoryRepositories();
            return services;
        }

        var connStr = connectionString
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("DbConnectionString")
            ?? string.Empty;

        return services.AddDatabaseRepositories(connStr);
    }

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
