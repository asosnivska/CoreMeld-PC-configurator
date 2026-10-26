using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryComponentRepository : IComponentRepository
{
    public Task<IEnumerable<Component>> GetAllAsync()
        => Task.FromResult<IEnumerable<Component>>(InMemoryCatalog.All);

    public Task<Component?> GetByIdAsync(int id)
        => Task.FromResult(InMemoryCatalog.All.FirstOrDefault(c => c.Id == id));
}