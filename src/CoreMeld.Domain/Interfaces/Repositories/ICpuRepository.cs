using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;

public interface ICpuRepository
{
    Task<Cpu?> GetByIdAsync(int id);
    Task<IEnumerable<Cpu>> GetByFilterAsync(
        
        int? socketId = null,
        int? minCores = null,
        int? minThreads = null,
        bool? hasIntegratedGpu = null,
        int? maxTdpWatts = null,
        
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
    );
}