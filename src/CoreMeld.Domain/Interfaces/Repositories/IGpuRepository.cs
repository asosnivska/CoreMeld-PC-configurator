using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;
public interface IGpuRepository
{
    Task<Gpu?> GetByIdAsync(int id);
    Task<IEnumerable<Gpu>> GetByFilterAsync(
        
        int? minVramGb = null,
        int? maxRecommendedPsuWatts = null,
        IEnumerable<string>? memoryType = null,
        int? maxTdpWatts = null,
        int? maxLengthMm = null,
        
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        );
}