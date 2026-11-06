using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Services;

public interface IBudgetCalculator
{
    IReadOnlyDictionary<ComponentCategory, decimal> CalculateAllocation(decimal totalBudget,
        IReadOnlyCollection<Category> categories);

    Task<ThresholdResult> CheckThresholdAsync(decimal budget, IReadOnlyCollection<Category> categories);
}