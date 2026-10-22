using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IStorageRepository
{
    Task<Storage?> GetByIdAsync(int id);
    Task<IEnumerable<Storage>> GetByFilterAsync(
        
        IEnumerable<StorageType>? driveType = null,
        int? capacityGb = null,
        IEnumerable<string>? interfaces = null,

        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        );
}