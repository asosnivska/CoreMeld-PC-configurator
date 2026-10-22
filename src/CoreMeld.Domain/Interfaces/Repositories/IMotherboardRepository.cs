using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IMotherboardRepository
{
    Task<Motherboard?> GetByIdAsync(int id);
    Task<IEnumerable<Motherboard>> GetByFilterAsync(
        int? socketId = null,
        string? chipset = null,
        int? formFactorId = null,
        string? memoryType = null,
        int? minMemorySlots = null,
        int? minSupportedMemoryGb = null,
        bool? hasWifi = null,

        
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        
        
        );
}