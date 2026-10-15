using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;
public interface IRamRepository
{
    Task<Ram?> GetByIdAsync(int id);
    Task<IEnumerable<Ram>> GetByFilterAsync(
        
        IEnumerable<string>? memoryType = null,
        int? minCapacityGb = null,
        int? maxModuleCount = null,
        int? minFrequencyMhz = null,
 
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        );
}