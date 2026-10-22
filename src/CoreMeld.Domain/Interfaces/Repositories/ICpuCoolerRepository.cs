
using CoreMeld.Domain.Models;

namespace CoreMeld.Domain.Interfaces.Repositories;

public interface ICpuCoolerRepository
{
    Task<CpuCooler?> GetByIdAsync(int id);
    Task<IEnumerable<CpuCooler>> GetByFilterAsync(
            int? socketId = null,
            int? minTdpRatingWatts = null,
            int? maxHeightMm = null,
            decimal? maxNoiseLevelDb = null,
            CoolingType? coolingType = null,
        
            decimal? minPrice = null,
            decimal? maxPrice = null,
            IEnumerable<string>? brands = null,
            string? nameContains = null,
            int? limit = null
        );
    
}
