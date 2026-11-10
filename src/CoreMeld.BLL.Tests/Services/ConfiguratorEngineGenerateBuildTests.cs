using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Exceptions;
using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using static CoreMeld.BLL.Tests.Fakes.FakeAllocationSettingsProvider;

namespace CoreMeld.BLL.Tests.Services;

public class ConfiguratorEngineGenerateBuildTests
{
    private static ConfiguratorEngine CreateEngine()
        => EngineFactory.Create(new()
        {
            [Category.GamingFullHd] = Weights(
                cpu: 20, motherboard: 10, gpu: 35, ram: 10, storage: 10, cpuCooler: 5, pcCase: 4, powerSupply: 6),
            [Category.Office] = Weights(
                cpu: 35, motherboard: 15, ram: 15, storage: 15, cpuCooler: 5, pcCase: 6, powerSupply: 9)
        });

    private static BuildRequestDto Request(
        decimal budget,
        Category? category = Category.GamingFullHd,
        bool? requireWifi = null,
        string? color = null,
        int? ramGb = null)
        => new(budget, color, requireWifi, null, null, category, null, ramGb, null);

    [Fact]
    public async Task GenerateBuildAsync_NullRequest_Throws()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.GenerateBuildAsync(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GenerateBuildAsync_BudgetNotPositive_Throws(int budget)
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentException>(() => engine.GenerateBuildAsync(Request(budget)));
    }

    [Fact]
    public async Task GenerateBuildAsync_CategoryMissing_Throws()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentException>(
            () => engine.GenerateBuildAsync(Request(35000m, category: null)));
    }

    [Fact]
    public async Task GenerateBuildAsync_BudgetBelowMinimum_ThrowsWithShortfall()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(22000m)));

        Assert.False(exception.Result.IsSufficient);
        Assert.Equal(22203m, exception.Result.MinimumBudget);
        Assert.Equal(203m, exception.Result.Shortfall);
    }

    [Fact]
    public async Task GenerateBuildAsync_Gaming_ReturnsCompatibleBuildWithGpuWithinBudget()
    {
        var engine = CreateEngine();

        var result = await engine.GenerateBuildAsync(Request(35000m));

        Assert.True(result.IsCompatible);
        Assert.True(result.TotalPrice <= 35000m);
        Assert.Equal(8, result.Components.Count);
        Assert.Contains(result.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_Gaming_ContainsEveryCategoryOnce()
    {
        var engine = CreateEngine();

        var result = await engine.GenerateBuildAsync(Request(35000m));

        var categories = result.Components.Select(c => c.Category).OrderBy(c => c).ToArray();
        Assert.Equal(Enum.GetValues<ComponentCategory>().OrderBy(c => c).ToArray(), categories);
    }

    [Fact]
    public async Task GenerateBuildAsync_Office_ReturnsBuildWithoutGpuAndWithIntegratedGraphics()
    {
        var engine = CreateEngine();

        var result = await engine.GenerateBuildAsync(Request(27000m, Category.Office));

        Assert.True(result.IsCompatible);
        Assert.True(result.TotalPrice <= 27000m);
        Assert.Equal(7, result.Components.Count);
        Assert.DoesNotContain(result.Components, c => c.Category == ComponentCategory.Gpu);

        var cpu = Assert.Single(result.Components, c => c.Category == ComponentCategory.Cpu);
        var integratedGraphics = Assert.Single(cpu.Characteristics, c => c.Name == "Вбудована графіка");
        Assert.Equal("Так", integratedGraphics.Value);
    }

    [Fact]
    public async Task GenerateBuildAsync_TotalPriceEqualsSumOfComponentPrices()
    {
        var engine = CreateEngine();

        var result = await engine.GenerateBuildAsync(Request(35000m));

        Assert.Equal(result.Components.Sum(c => c.Price), result.TotalPrice);
    }

    [Fact]
    public async Task GenerateBuildAsync_TotalTdpIsPositive()
    {
        var engine = CreateEngine();

        var result = await engine.GenerateBuildAsync(Request(35000m));

        Assert.True(result.TotalTdp > 0);
    }

    [Fact]
    public async Task GenerateBuildAsync_CheapestPartsExceedBudget_ThrowsInsufficientBudget()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(17000m, Category.Office)));

        Assert.False(exception.Result.IsSufficient);
        Assert.True(exception.Result.Shortfall > 0m);
    }

    [Fact]
    public async Task GenerateBuildAsync_RequestedRamExceedsAnyAvailable_ThrowsInvalidOperation()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.GenerateBuildAsync(Request(35000m, ramGb: 1000)));
    }

    [Fact]
    public async Task GenerateBuildAsync_NoCaseInPreferredColor_ThrowsInvalidOperation()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.GenerateBuildAsync(Request(35000m, color: "Pink")));
    }

    [Fact]
    public async Task GenerateBuildAsync_WifiRequiredButNoSuchBoardForChosenCpu_ThrowsInvalidOperation()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => engine.GenerateBuildAsync(Request(27000m, Category.Office, requireWifi: true)));
    }
}