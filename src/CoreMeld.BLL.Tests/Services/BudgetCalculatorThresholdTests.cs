using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using Xunit;
using static CoreMeld.BLL.Tests.Fakes.FakeAllocationSettingsProvider;

namespace CoreMeld.BLL.Tests.Services;

public class BudgetCalculatorThresholdTests
{
    private static Dictionary<ComponentCategory, decimal> DefaultPrices() => new()
    {
        [ComponentCategory.Cpu] = 2000m,
        [ComponentCategory.Gpu] = 4000m,
        [ComponentCategory.Ram] = 1000m
    };

    private static (BudgetCalculator Calculator, FakeComponentRepository Repository) Create(
        Dictionary<ComponentCategory, decimal> prices)
    {
        var provider = new FakeAllocationSettingsProvider(new()
        {
            [Category.Office] = Weights(cpu: 50, gpu: 50),
            [Category.GamingFullHd] = Weights(cpu: 60, ram: 40)
        });

        var repository = new FakeComponentRepository(prices);

        return (new BudgetCalculator(provider, repository), repository);
    }

    [Fact]
    public async Task CheckThresholdAsync_BudgetEqualsMinimum_IsSufficient()
    {
        var (calculator, _) = Create(DefaultPrices());

        var result = await calculator.CheckThresholdAsync(6000m, new[] { Category.Office });

        Assert.Equal(new ThresholdResult(true, 6000m, 0m), result);
    }

    [Fact]
    public async Task CheckThresholdAsync_BudgetBelowMinimum_ReturnsShortfall()
    {
        var (calculator, _) = Create(DefaultPrices());

        var result = await calculator.CheckThresholdAsync(5999m, new[] { Category.Office });

        Assert.Equal(new ThresholdResult(false, 6000m, 1m), result);
    }

    [Fact]
    public async Task CheckThresholdAsync_BudgetAboveMinimum_IsSufficient()
    {
        var (calculator, _) = Create(DefaultPrices());

        var result = await calculator.CheckThresholdAsync(10000m, new[] { Category.Office });

        Assert.Equal(new ThresholdResult(true, 6000m, 0m), result);
    }

    [Fact]
    public async Task CheckThresholdAsync_ZeroWeightComponent_IsNotQueried()
    {
        var (calculator, repository) = Create(DefaultPrices());

        await calculator.CheckThresholdAsync(6000m, new[] { Category.Office });

        Assert.DoesNotContain(ComponentCategory.Ram, repository.Calls);
        Assert.Equal(2, repository.Calls.Count);
    }

    [Fact]
    public async Task CheckThresholdAsync_MixedProfiles_SumsAllNeededComponents()
    {
        var (calculator, _) = Create(DefaultPrices());

        var result = await calculator.CheckThresholdAsync(
            7000m,
            new[] { Category.Office, Category.GamingFullHd });

        Assert.Equal(new ThresholdResult(true, 7000m, 0m), result);
    }

    [Fact]
    public async Task CheckThresholdAsync_MissingPrice_Throws()
    {
        var prices = new Dictionary<ComponentCategory, decimal>
        {
            [ComponentCategory.Cpu] = 2000m
        };
        var (calculator, _) = Create(prices);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => calculator.CheckThresholdAsync(6000m, new[] { Category.Office }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckThresholdAsync_NonPositiveBudget_Throws(decimal budget)
    {
        var (calculator, _) = Create(DefaultPrices());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => calculator.CheckThresholdAsync(budget, new[] { Category.Office }));
    }

    [Fact]
    public async Task CheckThresholdAsync_EmptyCategories_Throws()
    {
        var (calculator, _) = Create(DefaultPrices());

        await Assert.ThrowsAsync<ArgumentException>(
            () => calculator.CheckThresholdAsync(6000m, Array.Empty<Category>()));
    }
}