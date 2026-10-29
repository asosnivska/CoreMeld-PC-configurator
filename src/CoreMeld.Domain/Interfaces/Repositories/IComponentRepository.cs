using CoreMeld.Domain.Models;

namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IComponentRepository
{
    Task<IEnumerable<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<IEnumerable<Component>> GetByCategoryAsync(ComponentCategory category)
        => Task.FromResult<IEnumerable<Component>>(Enumerable.Empty<Component>());

    Task<IEnumerable<Component>> GetFilteredAsync(
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? manufacturers = null,
        int? socketId = null,
        string? chipset = null,
        bool? sortByPriceAsc = null,
        int? categoryId = null,
        int? limit = null)
        => Task.FromResult<IEnumerable<Component>>(Enumerable.Empty<Component>());
}
