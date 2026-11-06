using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Fakes;

public class FakeComponentRepository: IComponentRepository
{
    private readonly Dictionary<ComponentCategory, decimal> _minPrices;

    public List<ComponentCategory> Calls { get; } = new();


    public FakeComponentRepository(Dictionary<ComponentCategory, decimal>? minPrices = null)
    {
        _minPrices = minPrices ?? new Dictionary<ComponentCategory, decimal>();
    }

    public Task<IEnumerable<Component>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Component?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<decimal?> GetMinPriceAsync(ComponentCategory category)
    {
        Calls.Add(category);

        decimal? price;
        if (_minPrices.TryGetValue(category, out var value))
        {
            price = value;
        }
        else
        {
            price = null;
        }

        return Task.FromResult(price);
    }
}