using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Settings;

public interface IAllocationSettingsProvider
{
    IReadOnlyDictionary<ComponentCategory, decimal> GetWeights(Category category);
}