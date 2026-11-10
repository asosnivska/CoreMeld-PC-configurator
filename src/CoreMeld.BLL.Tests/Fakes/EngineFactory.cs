using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings;
using CoreMeld.DAL.InMemory;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Tests.Fakes;

public static class EngineFactory
{
    public static ConfiguratorEngine Create(
        Dictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>>? weights = null)
    {
        var formFactors = new FakeFormFactorSpecsProvider(new Dictionary<int, BoardSize>
        {
            [1] = new BoardSize(305, 244),
            [2] = new BoardSize(244, 244),
            [3] = new BoardSize(170, 170)
        });

        var budgetCalculator = new BudgetCalculator(
            new FakeAllocationSettingsProvider(
                weights ?? new Dictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>>()),
            new InMemoryComponentRepository());

        return new ConfiguratorEngine(
            budgetCalculator,
            new CompatibilityChecker(),
            new TdpCalculator(),
            new SpaceControlService(formFactors),
            new InMemoryCpuRepository(),
            new InMemoryMotherboardRepository(),
            new InMemoryGpuRepository(),
            new InMemoryRamRepository(),
            new InMemoryStorageRepository(),
            new InMemoryCpuCoolerRepository(),
            new InMemoryPCCaseRepository(),
            new InMemoryPowerSupplyRepository());
    }
}