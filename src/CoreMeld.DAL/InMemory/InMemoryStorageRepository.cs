using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryStorageRepository : IStorageRepository
{
    public Task<Storage?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Storages.FirstOrDefault(s => s.Id == id));

    public Task<IEnumerable<Storage>> GetByFilterAsync(
        IEnumerable<StorageType>? driveType = null,
        int? capacityGb = null,
        IEnumerable<string>? interfaces = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Storages.AsEnumerable();

        if (driveType is not null)
        {
            var names = driveType.Select(t => t.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (names.Count > 0) query = query.Where(s => names.Contains(s.DriveType));
        }
        if (capacityGb.HasValue) query = query.Where(s => s.CapacityGb >= capacityGb.Value);
        if (interfaces is not null)
        {
            var set = interfaces.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (set.Count > 0) query = query.Where(s => set.Contains(s.InterfaceType));
        }

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}