using CoreMeld.Domain.Models;

namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IComponentRepository
{
    Task<IEnumerable<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<IEnumerable<Component>> GetByCategoryAsync(ComponentCategory category)
        => Task.FromResult<IEnumerable<Component>>(Enumerable.Empty<Component>());
}
