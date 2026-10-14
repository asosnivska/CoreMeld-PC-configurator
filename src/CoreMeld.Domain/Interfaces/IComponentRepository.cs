using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.Interfaces;

public interface IComponentRepository
{
    Task<IEnumerable<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
}
