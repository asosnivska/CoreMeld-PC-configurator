using CoreMeld.BLL.Settings;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Tests.Fakes;

public class FakeAllocationSettingsProvider: IAllocationSettingsProvider
{
    private readonly Dictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>> _weights;

    public FakeAllocationSettingsProvider(Dictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>> weights)
    {
        _weights = weights;
    }

    public IReadOnlyDictionary<ComponentCategory, decimal> GetWeights(Category category)
    {
        return _weights[category];
    }

    public static IReadOnlyDictionary<ComponentCategory, decimal> Weights(
        decimal cpu = 0m,
        decimal motherboard = 0m,
        decimal gpu = 0m,
        decimal ram = 0m,
        decimal storage = 0m,
        decimal cpuCooler = 0m,
        decimal pcCase = 0m,
        decimal powerSupply = 0m)
    {
        return new Dictionary<ComponentCategory, decimal>
        {
            [ComponentCategory.Cpu] = cpu,
            [ComponentCategory.Motherboard] = motherboard,
            [ComponentCategory.Gpu] = gpu,
            [ComponentCategory.Ram] = ram,
            [ComponentCategory.Storage] = storage,
            [ComponentCategory.CpuCooler] = cpuCooler,
            [ComponentCategory.Case] = pcCase,
            [ComponentCategory.PowerSupply] = powerSupply
        };
    }
}