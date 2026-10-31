using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using Xunit;
using static CoreMeld.BLL.Tests.Fakes.FakeAllocationSettingsProvider;

namespace CoreMeld.BLL.Tests.Services;

public class BudgetCalculatorAllocationTests
{
    private static BudgetCalculator CreateCalculator()
    {
        var provider = new FakeAllocationSettingsProvider(new()
        {
            [Category.Office] = Weights(cpu: 50, gpu: 50),
            [Category.GamingFullHd] = Weights(cpu: 60, ram: 40),
            [Category.Home2K] = Weights(gpu: 100),
            [Category.Gaming2K] = Weights(ram: 100),
            [Category.Design2D] = Weights(storage: 100)
        });

        return new BudgetCalculator(provider, new FakeComponentRepository());
    }

    private static void AssertAllocation(
        IReadOnlyDictionary<ComponentCategory, decimal> actual,
        decimal cpu = 0m,
        decimal motherboard = 0m,
        decimal gpu = 0m,
        decimal ram = 0m,
        decimal storage = 0m,
        decimal cpuCooler = 0m,
        decimal pcCase = 0m,
        decimal powerSupply = 0m)
    {
        Assert.Equal(cpu, actual[ComponentCategory.Cpu]);
        Assert.Equal(motherboard, actual[ComponentCategory.Motherboard]);
        Assert.Equal(gpu, actual[ComponentCategory.Gpu]);
        Assert.Equal(ram, actual[ComponentCategory.Ram]);
        Assert.Equal(storage, actual[ComponentCategory.Storage]);
        Assert.Equal(cpuCooler, actual[ComponentCategory.CpuCooler]);
        Assert.Equal(pcCase, actual[ComponentCategory.Case]);
        Assert.Equal(powerSupply, actual[ComponentCategory.PowerSupply]);
    }

    [Fact]
    public void CalculateAllocation_SingleProfile_SplitsByWeights()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(1000m, new[] { Category.Office });

        AssertAllocation(result, cpu: 500m, gpu: 500m);
    }

    [Fact]
    public void CalculateAllocation_MixedProfiles_UsesMaxWeightsAndNormalizes()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(1500m, new[] { Category.Office, Category.GamingFullHd });

        AssertAllocation(result, cpu: 600m, gpu: 500m, ram: 400m);
    }

    [Fact]
    public void CalculateAllocation_ProfileOrderChanged_GivesSameResult()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(1500m, new[] { Category.GamingFullHd, Category.Office });

        AssertAllocation(result, cpu: 600m, gpu: 500m, ram: 400m);
    }

    [Fact]
    public void CalculateAllocation_DuplicateProfile_SameAsSingle()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(1000m, new[] { Category.Office, Category.Office });

        AssertAllocation(result, cpu: 500m, gpu: 500m);
    }

    [Fact]
    public void CalculateAllocation_EqualShares_RemainderGoesToFirstComponent()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(
            100m,
            new[] { Category.Home2K, Category.Gaming2K, Category.Design2D });

        AssertAllocation(result, gpu: 33.34m, ram: 33.33m, storage: 33.33m);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(7)]
    [InlineData(1)]
    public void CalculateAllocation_AnyBudget_SumEqualsBudget(decimal budget)
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(
            budget,
            new[] { Category.Office, Category.GamingFullHd, Category.Home2K });

        Assert.Equal(budget, result.Values.Sum());
    }

    [Fact]
    public void CalculateAllocation_ValidInput_ContainsAllComponents()
    {
        var calculator = CreateCalculator();

        var result = calculator.CalculateAllocation(1000m, new[] { Category.Office });

        Assert.Equal(8, result.Count);
    }

    [Fact]
    public void CalculateAllocation_EmptyCategories_Throws()
    {
        var calculator = CreateCalculator();

        Assert.Throws<ArgumentException>(() => calculator.CalculateAllocation(1000m, Array.Empty<Category>()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateAllocation_NonPositiveBudget_Throws(decimal budget)
    {
        var calculator = CreateCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.CalculateAllocation(budget, new[] { Category.Office }));
    }
}