using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryCpuCoolerRepository : ICpuCoolerRepository
{
    public Task<CpuCooler?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.CpuCoolers.FirstOrDefault(c => c.Id == id));

    public Task<IEnumerable<CpuCooler>> GetByFilterAsync(
        int? socketId = null,
        int? minTdpRatingWatts = null,
        int? maxHeightMm = null,
        decimal? maxNoiseLevelDb = null,
        CoolingType? coolingType = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var query = InMemoryCatalog.CpuCoolers.AsEnumerable();

        if (socketId.HasValue) query = query.Where(c => c.SocketId == socketId.Value);
        if (minTdpRatingWatts.HasValue) query = query.Where(c => c.TdpRatingWatts >= minTdpRatingWatts.Value);
        if (maxHeightMm.HasValue) query = query.Where(c => c.HeightMm <= maxHeightMm.Value);
        if (maxNoiseLevelDb.HasValue) query = query.Where(c => c.NoiseLevelDb <= maxNoiseLevelDb.Value);
        // TODO: coolingType ігнорується, бо у CpuCooler немає поля типу охолодження

        return Task.FromResult(query.ApplyCommon(minPrice, maxPrice, brands, nameContains, limit));
    }
}