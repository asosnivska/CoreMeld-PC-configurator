using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryPowerSupplyRepository : IPowerSupplyRepository
{
    public Task<PowerSupply?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.PowerSupplies.FirstOrDefault(p => p.Id == id));

    public Task<IEnumerable<PowerSupply>> GetByFilterAsync(
        int? formFactorId = null,
        int? wattage = null,
        string? efficiencyRating = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.PowerSupplies.AsEnumerable();

        if (formFactorId.HasValue) query = query.Where(p => p.FormFactorId == formFactorId.Value);
        if (wattage.HasValue) query = query.Where(p => p.Wattage >= wattage.Value);
        if (!string.IsNullOrWhiteSpace(efficiencyRating))
            query = query.Where(p => p.EfficiencyRating.Equals(efficiencyRating, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}