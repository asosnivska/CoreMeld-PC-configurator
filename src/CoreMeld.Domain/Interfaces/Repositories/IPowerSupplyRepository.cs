using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IPowerSupplyRepository
{
    Task<PowerSupply?> GetByIdAsync(int id);
    Task<IEnumerable<PowerSupply>> GetByFilterAsync(
        
        int? formFactorId = null,
        int? wattage = null,
        string? efficiencyRating = null,
        
        
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        
        );
}