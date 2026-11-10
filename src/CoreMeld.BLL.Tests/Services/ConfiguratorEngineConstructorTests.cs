using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.DAL.InMemory;
using CoreMeld.Domain.Interfaces.Repositories;

namespace CoreMeld.BLL.Tests.Services;

public class ConfiguratorEngineConstructorTests
{
    private static readonly string[] ParameterNames =
    {
        "budgetCalculator",
        "compatibilityChecker",
        "tdpCalculator",
        "spaceControlService",
        "cpuRepository",
        "motherboardRepository",
        "gpuRepository",
        "ramRepository",
        "storageRepository",
        "cpuCoolerRepository",
        "pcCaseRepository",
        "powerSupplyRepository"
    };

    public static IEnumerable<object[]> ParameterIndexes()
        => Enumerable.Range(0, ParameterNames.Length).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(ParameterIndexes))]
    public void Constructor_NullDependency_ThrowsWithParameterName(int index)
    {
        var formFactors = new FakeFormFactorSpecsProvider(new Dictionary<int, BoardSize>
        {
            [1] = new BoardSize(305, 244)
        });

        object?[] args =
        {
            new BudgetCalculator(new FakeAllocationSettingsProvider(new()), new InMemoryComponentRepository()),
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
            new InMemoryPowerSupplyRepository()
        };
        args[index] = null;

        var exception = Assert.Throws<ArgumentNullException>(() => new ConfiguratorEngine(
            (IBudgetCalculator)args[0]!,
            (ICompatibilityChecker)args[1]!,
            (ITdpCalculator)args[2]!,
            (ISpaceControlService)args[3]!,
            (ICpuRepository)args[4]!,
            (IMotherboardRepository)args[5]!,
            (IGpuRepository)args[6]!,
            (IRamRepository)args[7]!,
            (IStorageRepository)args[8]!,
            (ICpuCoolerRepository)args[9]!,
            (IPCCaseRepository)args[10]!,
            (IPowerSupplyRepository)args[11]!));

        Assert.Equal(ParameterNames[index], exception.ParamName);
    }
}