using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryGpuRepository : IGpuRepository
{
    public Task<Gpu?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Gpus.FirstOrDefault(g => g.Id == id));

    public Task<IEnumerable<Gpu>> GetByFilterAsync(
        int? minVramGb = null,
        int? maxRecommendedPsuWatts = null,
        IEnumerable<VideoMemoryType>? memoryType = null,
        int? maxTdpWatts = null,
        int? maxLengthMm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Gpus.AsEnumerable();

        if (minVramGb.HasValue) query = query.Where(g => g.VramGb >= minVramGb.Value);
        if (maxRecommendedPsuWatts.HasValue) query = query.Where(g => g.RecommendedPsuWatts <= maxRecommendedPsuWatts.Value);
        if (memoryType is not null)
        {
            var types = memoryType.ToList();
            if (types.Count > 0) query = query.Where(g => types.Contains(g.MemoryType));
        }
        if (maxTdpWatts.HasValue) query = query.Where(g => g.TdpWatts <= maxTdpWatts.Value);
        if (maxLengthMm.HasValue) query = query.Where(g => g.LengthMm <= maxLengthMm.Value);

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}