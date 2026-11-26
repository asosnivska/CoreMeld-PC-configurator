using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Exceptions;
using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.DAL.InMemory;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using static CoreMeld.BLL.Tests.Fakes.FakeAllocationSettingsProvider;

namespace CoreMeld.BLL.Tests.Integration;

public class ConfiguratorEngineIntegrationTests
{
    private static Dictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>> TestWeights() => new()
    {
        [Category.GamingFullHd] = Weights(
            cpu: 20, motherboard: 10, gpu: 35, ram: 10, storage: 10, cpuCooler: 5, pcCase: 4, powerSupply: 6),
        [Category.Office] = Weights(
            cpu: 35, motherboard: 15, ram: 15, storage: 15, cpuCooler: 5, pcCase: 6, powerSupply: 9)
    };

    private static ConfiguratorEngine CreateEngine() => EngineFactory.Create(TestWeights());

    private static ConfiguratorEngine CreateEngineWithEmptyCpuTable()
    {
        var formFactors = new FakeFormFactorSpecsProvider(new Dictionary<int, BoardSize>
        {
            [1] = new BoardSize(305, 244),
            [2] = new BoardSize(244, 244),
            [3] = new BoardSize(170, 170)
        });

        var calculator = new BudgetCalculator(
            new FakeAllocationSettingsProvider(TestWeights()),
            new InMemoryComponentRepository());

        return new ConfiguratorEngine(
            calculator,
            new CompatibilityChecker(),
            new TdpCalculator(),
            new SpaceControlService(formFactors),
            new EmptyCpuRepository(),
            new InMemoryMotherboardRepository(),
            new InMemoryGpuRepository(),
            new InMemoryRamRepository(),
            new InMemoryStorageRepository(),
            new InMemoryCpuCoolerRepository(),
            new InMemoryPCCaseRepository(),
            new InMemoryPowerSupplyRepository());
    }

    private static BuildRequestDto Request(decimal budget, Category category = Category.GamingFullHd)
        => new(budget, null, null, null, null, category, null, null, null);

    private sealed class EmptyCpuRepository : ICpuRepository
    {
        public Task<Cpu?> GetByIdAsync(int id) => Task.FromResult<Cpu?>(null);

        public Task<IEnumerable<Cpu>> GetByFilterAsync(
            int? socketId = null,
            int? minCores = null,
            int? minThreads = null,
            bool? hasIntegratedGpu = null,
            int? maxTdpWatts = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            IEnumerable<string>? brands = null,
            string? nameContains = null,
            int? limit = null)
            => Task.FromResult<IEnumerable<Cpu>>(Array.Empty<Cpu>());
    }

    [Fact]
    public async Task Generate_ReturnsOnlyComponentsFromCatalog()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(Request(35000m));

        foreach (var item in build.Components)
        {
            var fromCatalog = InMemoryCatalog.All.Single(c => c.Id == item.Id);
            Assert.Equal(fromCatalog.Price, item.Price);
        }
    }

    [Fact]
    public async Task Generate_ThenReplaceGpu_KeepsBuildConsistent()
    {
        var engine = CreateEngine();
        var build = await engine.GenerateBuildAsync(Request(100000m));
        var currentGpuId = build.Components.Single(c => c.Category == ComponentCategory.Gpu).Id;
        var otherGpu = InMemoryCatalog.Gpus.OrderBy(g => g.Price).First(g => g.Id != currentGpuId);

        var replaced = await engine.ReplaceComponentAsync(build, ComponentCategory.Gpu, otherGpu);

        Assert.Equal(otherGpu.Id, replaced.Components.Single(c => c.Category == ComponentCategory.Gpu).Id);
        Assert.Equal(build.Components.Count, replaced.Components.Count);
        Assert.Equal(replaced.Components.Sum(c => c.Price), replaced.TotalPrice);
    }

    [Fact]
    public async Task Generate_ThenFilterCpus_ReturnsOnlyCpusWithBoardSocket_AndKeepsCurrent()
    {
        var engine = CreateEngine();
        var build = await engine.GenerateBuildAsync(Request(100000m));
        var boardId = build.Components.Single(c => c.Category == ComponentCategory.Motherboard).Id;
        var boardSocket = InMemoryCatalog.Motherboards.Single(m => m.Id == boardId).SocketId;
        var currentCpuId = build.Components.Single(c => c.Category == ComponentCategory.Cpu).Id;

        var compatible = await engine.FilterCompatibleComponentsAsync(
            InMemoryCatalog.Cpus.Cast<Component>().ToList(), build);

        Assert.NotEmpty(compatible);
        Assert.All(compatible, c => Assert.Equal(boardSocket, ((Cpu)c).SocketId));
        Assert.Contains(compatible, c => c.Id == currentCpuId);
    }

    [Fact]
    public async Task Generate_ThenReplaceWithCpuOfOtherSocket_MarksBuildIncompatible()
    {
        var engine = CreateEngine();
        var build = await engine.GenerateBuildAsync(Request(100000m));
        var boardId = build.Components.Single(c => c.Category == ComponentCategory.Motherboard).Id;
        var boardSocket = InMemoryCatalog.Motherboards.Single(m => m.Id == boardId).SocketId;
        var wrongCpu = InMemoryCatalog.Cpus.First(c => c.SocketId != boardSocket);

        var replaced = await engine.ReplaceComponentAsync(build, ComponentCategory.Cpu, wrongCpu);

        Assert.False(replaced.IsCompatible);
    }

    [Fact]
    public async Task EdgeCase_BudgetZero_Throws()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentException>(() => engine.GenerateBuildAsync(Request(0m)));
    }

    [Fact]
    public async Task EdgeCase_BudgetBelowMinimum_ReportsRealMinimum()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(18000m)));

        Assert.Equal(18598m, exception.Result.MinimumBudget);
        Assert.Equal(598m, exception.Result.Shortfall);
    }

    [Theory]
    [InlineData(Category.GamingFullHd)]
    [InlineData(Category.Office)]
    public async Task EdgeCase_VeryLargeBudget_BuildsCompatiblePc(Category category)
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(Request(10_000_000m, category));

        Assert.True(build.IsCompatible);
        Assert.True(build.TotalPrice <= 10_000_000m);
    }

    [Fact]
    public async Task EdgeCase_EmptyDatabase_ThrowsInvalidOperation()
    {
        var engine = CreateEngineWithEmptyCpuTable();

        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.GenerateBuildAsync(Request(100000m)));
    }
}