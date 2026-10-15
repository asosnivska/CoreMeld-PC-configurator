using CoreMeld.Domain.Models;
namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IPCCaseRepository
{
    Task<PCCase?> GetByIdAsync(int id);
    Task<IEnumerable<PCCase>> GetByFilterAsync(
        
        int? supportedMotherboardFormFactorId = null,
        int? minSupportedGpuLengthMm = null,
        int? minSupportedCoolerHeightMm = null,
        string? color = null,
        
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null
        
        );
    
}