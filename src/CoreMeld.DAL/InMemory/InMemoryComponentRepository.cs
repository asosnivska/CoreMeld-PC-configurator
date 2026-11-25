using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryComponentRepository : IComponentRepository
{
    public Task<IEnumerable<Component>> GetAllAsync()
        => Task.FromResult<IEnumerable<Component>>(InMemoryCatalog.All);

    public Task<Component?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.All.FirstOrDefault(c => c.Id == id));

    public Task<decimal?> GetMinPriceAsync(ComponentCategory category)
    {
        var prices = InMemoryCatalog.All
            .Where(c => c.CategoryId == (int)category)
            .Select(c => c.Price)
            .ToList();

        return Task.FromResult<decimal?>(prices.Count == 0 ? null : prices.Min());
    }

    public Task<IEnumerable<Component>> SearchByNameAsync(string query, int? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult<IEnumerable<Component>>(Array.Empty<Component>());

        var text = query.Trim();
        var result = InMemoryCatalog.All
            .Where(c => (categoryId is null || c.CategoryId == categoryId)
                        && c.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<Component>>(result);
    }
}