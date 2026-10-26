using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryMotherboardRepository : IMotherboardRepository
{
    public Task<Motherboard?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Motherboards.FirstOrDefault(m => m.Id == id));

    public Task<IEnumerable<Motherboard>> GetByFilterAsync(
        int? socketId = null,
        string? chipset = null,
        int? formFactorId = null,
        IEnumerable<RamMemoryType>? memoryType = null,
        int? minMemorySlots = null,
        int? minSupportedMemoryGb = null,
        bool? hasWifi = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Motherboards.AsEnumerable();

        if (socketId.HasValue) query = query.Where(m => m.SocketId == socketId.Value);
        if (!string.IsNullOrWhiteSpace(chipset))
            query = query.Where(m => m.Chipset.Equals(chipset, StringComparison.OrdinalIgnoreCase));
        if (formFactorId.HasValue) query = query.Where(m => m.FormFactorId == formFactorId.Value);
        if (memoryType is not null)
        {
            var types = memoryType.ToList();
            if (types.Count > 0) query = query.Where(m => types.Contains(m.MemoryType));
        }
        if (minMemorySlots.HasValue) query = query.Where(m => m.MemorySlots >= minMemorySlots.Value);
        if (minSupportedMemoryGb.HasValue) query = query.Where(m => m.MaxMemoryGb >= minSupportedMemoryGb.Value);
        if (hasWifi.HasValue) query = query.Where(m => m.HasWifi == hasWifi.Value);

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}