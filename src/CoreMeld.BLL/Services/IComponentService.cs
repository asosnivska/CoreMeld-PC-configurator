using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface IComponentService
{
    Task<IReadOnlyList<ComponentDto>> GetComponentsByCategoryAsync(
        ComponentCategory category,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? sortByPriceAsc = null,
        IEnumerable<string>? brands = null);

    Task<IReadOnlyList<string>> GetBrandsByCategoryAsync(ComponentCategory category);
}

