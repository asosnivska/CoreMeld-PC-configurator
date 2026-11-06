using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryRamRepository : IRamRepository
{
    public Task<Ram?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Rams.FirstOrDefault(r => r.Id == id));

    public Task<IEnumerable<Ram>> GetByFilterAsync(
        IEnumerable<RamMemoryType>? memoryType = null,
        int? minCapacityGb = null,
        int? maxModuleCount = null,
        int? minFrequencyMhz = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Rams.AsEnumerable();

        if (memoryType is not null)
        {
            var names = memoryType.Select(t => t.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (names.Count > 0) query = query.Where(r => names.Contains(r.RamMemoryType));
        }
        if (minCapacityGb.HasValue) query = query.Where(r => r.CapacityGb >= minCapacityGb.Value);
        if (maxModuleCount.HasValue) query = query.Where(r => r.ModuleCount <= maxModuleCount.Value);
        if (minFrequencyMhz.HasValue) query = query.Where(r => r.FrequencyMhz >= minFrequencyMhz.Value);

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}