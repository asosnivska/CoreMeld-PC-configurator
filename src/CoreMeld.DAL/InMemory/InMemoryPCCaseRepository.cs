using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryPCCaseRepository : IPCCaseRepository
{
    public Task<PCCase?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.Cases.FirstOrDefault(c => c.Id == id));

    public Task<IEnumerable<PCCase>> GetByFilterAsync(
        int? supportedMotherboardFormFactorId = null,
        int? minSupportedGpuLengthMm = null,
        int? minSupportedCoolerHeightMm = null,
        string? color = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.Cases.AsEnumerable();

        // TODO: supportedMotherboardFormFactorId ігнорується: у PCCase немає підтримуваних форм-факторів
        if (minSupportedGpuLengthMm.HasValue) query = query.Where(c => c.MaxGpuLengthMm >= minSupportedGpuLengthMm.Value);
        if (minSupportedCoolerHeightMm.HasValue) query = query.Where(c => c.MaxCpuCoolerHeightMm >= minSupportedCoolerHeightMm.Value);
        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(c => c.Color.Equals(color, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}