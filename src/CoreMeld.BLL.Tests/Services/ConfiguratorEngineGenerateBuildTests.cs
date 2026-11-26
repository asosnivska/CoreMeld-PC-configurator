using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Exceptions;
using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using static CoreMeld.BLL.Tests.Fakes.FakeAllocationSettingsProvider;

namespace CoreMeld.BLL.Tests.Services;

// Мінімальні бюджети залежать від InMemoryCatalog (сума найдешевшої сумісної збірки):
// Gaming 18 598, Office 13 850, Office + Wi-Fi 15 650 (CPU 111, плата 203).
// Якщо змінити ціни чи склад каталогу, ці тести впадуть, і числа треба перерахувати.
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

    private static BuildRequestDto RequestWithTasks(
        decimal budget, Category? category, params UserTask[] tasks)
        => new(budget, null, null, null, null, category, tasks, null, null);

    private static int IdOf(PCBuildDto build, ComponentCategory category)
        => build.Components.Single(c => c.Category == category).Id;

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
    public async Task GenerateBuildAsync_NoCategoryAndEmptyPriorities_Throws()
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentException>(
            () => engine.GenerateBuildAsync(RequestWithTasks(35000m, null)));
    }

    [Fact]
    public async Task GenerateBuildAsync_PrioritiesOnly_UsesTasksInsteadOfThrowing()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(RequestWithTasks(60000m, null, UserTask.OfficeWork));

        Assert.True(build.IsCompatible);
        Assert.True(build.TotalPrice <= 60000m);
        Assert.DoesNotContain(build.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_TasksFromSeveralCategories_CombineWeights()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(
            RequestWithTasks(35000m, null, UserTask.OfficeWork, UserTask.Gaming1080p));

        Assert.True(build.IsCompatible);
        Assert.True(build.TotalPrice <= 35000m);
        Assert.Contains(build.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_CategoryAndTasks_AreCombined()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(
            RequestWithTasks(35000m, Category.Office, UserTask.Gaming1080p));

        Assert.True(build.IsCompatible);
        Assert.True(build.TotalPrice <= 35000m);
        Assert.Contains(build.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_TaskMatchesSelectedCategory_BuildsLikeCategoryAlone()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(
            RequestWithTasks(60000m, Category.Office, UserTask.OfficeWork));

        Assert.True(build.IsCompatible);
        Assert.DoesNotContain(build.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_CategoryOnlyWithEmptyTasks_UsesCategory()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(RequestWithTasks(35000m, Category.GamingFullHd));

        Assert.True(build.IsCompatible);
        Assert.Contains(build.Components, c => c.Category == ComponentCategory.Gpu);
    }

    [Fact]
    public async Task GenerateBuildAsync_BudgetBelowMinimum_ThrowsWithShortfall()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(18000m)));

        // 18598 = мінімальна збірка Gaming у InMemoryCatalog, 598 = 18598 - 18000
        Assert.False(exception.Result.IsSufficient);
        Assert.Equal(18598m, exception.Result.MinimumBudget);
        Assert.Equal(598m, exception.Result.Shortfall);
    }

    [Fact]
    public async Task GenerateBuildAsync_GamingAtMinimumBudget_Builds()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(Request(18598m));

        // 18598 = мінімальна збірка Gaming у InMemoryCatalog
        Assert.Equal(18598m, build.TotalPrice);
        Assert.True(build.IsCompatible);
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
    public async Task GenerateBuildAsync_OfficeAtMinimumBudget_Builds()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(Request(13850m, Category.Office));

        // 13850 = мінімальна збірка Office у InMemoryCatalog (найдешевший CPU з id 111)
        Assert.Equal(13850m, build.TotalPrice);
        Assert.Equal(111, IdOf(build, ComponentCategory.Cpu));
    }

    [Fact]
    public async Task GenerateBuildAsync_OfficeJustBelowMinimum_ReportsRealMinimum()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(13849m, Category.Office)));

        // 13850 = мінімум Office у InMemoryCatalog, нестача 1 грн
        Assert.Equal(13850m, exception.Result.MinimumBudget);
        Assert.Equal(1m, exception.Result.Shortfall);
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
    public async Task GenerateBuildAsync_CheapestPartsExceedBudget_ThrowsInsufficientBudgetWithRealMinimum()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(10000m, Category.Office)));

        // 13850 = мінімум Office у InMemoryCatalog, 3850 = 13850 - 10000
        Assert.False(exception.Result.IsSufficient);
        Assert.Equal(13850m, exception.Result.MinimumBudget);
        Assert.Equal(3850m, exception.Result.Shortfall);
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
    public async Task GenerateBuildAsync_OfficeWithWifiBelowMinimum_ReportsRealMinimumInsteadOfDeadEnd()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(14000m, Category.Office, requireWifi: true)));

        // 15650 = мінімум Office з Wi-Fi у InMemoryCatalog, 1650 = 15650 - 14000
        Assert.Equal(15650m, exception.Result.MinimumBudget);
        Assert.Equal(1650m, exception.Result.Shortfall);
    }

    [Fact]
    public async Task GenerateBuildAsync_OfficeWithWifiAtMinimumBudget_BuildsWithWifiBoard()
    {
        var engine = CreateEngine();

        var build = await engine.GenerateBuildAsync(Request(15650m, Category.Office, requireWifi: true));

        // 15650 = мінімум Office з Wi-Fi: CPU 111 + плата 203 (найдешевша з Wi-Fi)
        Assert.Equal(15650m, build.TotalPrice);
        Assert.Equal(111, IdOf(build, ComponentCategory.Cpu));
        Assert.Equal(203, IdOf(build, ComponentCategory.Motherboard));
    }

    [Fact]
    public async Task GenerateBuildAsync_OfficeWithWifiJustBelowMinimum_Throws()
    {
        var engine = CreateEngine();

        var exception = await Assert.ThrowsAsync<InsufficientBudgetException>(
            () => engine.GenerateBuildAsync(Request(15649m, Category.Office, requireWifi: true)));

        // 15650 = мінімум Office з Wi-Fi у InMemoryCatalog
        Assert.Equal(15650m, exception.Result.MinimumBudget);
    }

    [Theory]
    [InlineData(Category.Office)]
    [InlineData(Category.GamingFullHd)]
    public async Task GenerateBuildAsync_BudgetGrowth_OnceSucceedsNeverFailsAgain(Category category)
    {
        var engine = CreateEngine();
        var succeeded = false;

        for (var budget = 15000m; budget <= 100000m; budget += 500m)
        {
            PCBuildDto? build = null;
            try
            {
                build = await engine.GenerateBuildAsync(Request(budget, category));
            }
            catch (InsufficientBudgetException)
            {
            }

            if (build is not null)
            {
                succeeded = true;
                Assert.True(build.TotalPrice <= budget);
                Assert.True(build.IsCompatible);
            }
            else
            {
                Assert.False(succeeded, $"Бюджет {budget} впав після успіху на меншому бюджеті.");
            }
        }

        Assert.True(succeeded);
    }
}