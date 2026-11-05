
using System.Collections.ObjectModel;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Settings;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;

namespace CoreMeld.BLL.Services;

public class BudgetCalculator: IBudgetCalculator
{

    private readonly IAllocationSettingsProvider _provider;
    private readonly IComponentRepository _componentRepository;

    public BudgetCalculator(IAllocationSettingsProvider provider, IComponentRepository componentRepository)
    {
        _provider = provider;
        _componentRepository = componentRepository;
    }

    private Dictionary<ComponentCategory, decimal> GetMaxWeights(IReadOnlyCollection<Category> categories)
    {
        if (categories is null || categories.Count == 0)
        {
            throw new ArgumentException("Потрібно вибрати хоча б одну категорію.", nameof(categories));
        }

        var combined = new Dictionary<ComponentCategory, decimal>();
        foreach (var component in Enum.GetValues<ComponentCategory>())
        {
            combined[component] = 0m;
        }

        foreach (var category in categories.Distinct())
        {
            var weights = _provider.GetWeights(category);
            foreach (var (component, weight) in weights)
            {
                if (weight > combined[component])
                {
                    combined[component] = weight;
                }
            }
        }

        if (combined.Values.Sum() == 0m)
        {
            throw new InvalidOperationException("Сума ваг вибраних категорій дорівнює нулю.");
        }

        return combined;
    }
    
    
    public IReadOnlyDictionary<ComponentCategory, decimal> CalculateAllocation(decimal totalBudget, IReadOnlyCollection<Category> categories)
    {
        if (totalBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalBudget), totalBudget, "Бюджет має бути більшим за 0.");
        }

        var weights = GetMaxWeights(categories);
        var totalWeight = weights.Values.Sum();
        var components = Enum.GetValues<ComponentCategory>();

        var amounts = new Dictionary<ComponentCategory, decimal>();
        foreach (var component in components)
        {
           
            amounts[component] = Math.Floor(totalBudget * weights[component] * 100m / totalWeight) / 100m;
        }

        var remainder = totalBudget - amounts.Values.Sum();
        if (remainder > 0)
        {
           
            var leader = components[0];
            foreach (var component in components)
            {
                if (amounts[component] > amounts[leader])
                {
                    leader = component;
                }
            }

            amounts[leader] += remainder;
        }

        return new ReadOnlyDictionary<ComponentCategory, decimal>(amounts);
    }

    public async Task<ThresholdResult> CheckThresholdAsync(decimal budget, IReadOnlyCollection<Category> categories)
    {
        if (budget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(budget), budget, "Бюджет має бути більшим за 0.");
        }

        var weights = GetMaxWeights(categories);

        var minimumBudget = 0m;
        foreach (var (component, weight) in weights)
        {
            if (weight == 0m)
            {
                continue;
            }

            var minPrice = await _componentRepository.GetMinPriceAsync(component);
            if (minPrice is null)
            {
                throw new InvalidOperationException($"У базі немає жодної деталі категорії {component}.");
            }

            minimumBudget += minPrice.Value;
        }

        var shortfall = Math.Max(0m, minimumBudget - budget);
        return new ThresholdResult(shortfall == 0m, minimumBudget, shortfall);

    }
}