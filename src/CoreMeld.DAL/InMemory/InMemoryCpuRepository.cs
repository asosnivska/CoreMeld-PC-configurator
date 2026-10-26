using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryCpuRepository : ICpuRepository
{
    public Task<Cpu?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Cpus.FirstOrDefault(c => c.Id == id));

    public Task<IEnumerable<Cpu>> GetByFilterAsync(
        int? socketId = null,
        int? minCores = null,
        int? minThreads = null,
        bool? hasIntegratedGpu = null,
        int? maxTdpWatts = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Cpus.AsEnumerable();

        if (socketId.HasValue) query = query.Where(c => c.SocketId == socketId.Value);
        if (minCores.HasValue) query = query.Where(c => c.Cores >= minCores.Value);
        if (minThreads.HasValue) query = query.Where(c => c.Threads >= minThreads.Value);
        if (hasIntegratedGpu.HasValue) query = query.Where(c => c.HasIntegratedGpu == hasIntegratedGpu.Value);
        if (maxTdpWatts.HasValue) query = query.Where(c => c.TdpWatts <= maxTdpWatts.Value);

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}