using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.BLL.Validation;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Validation;

public class BuildRequestValidatorTests
{
    private static BuildRequestDto Request(
        decimal budget = 30000m,
        string? color = null,
        StorageType? storageType = null,
        CoolingType? coolingType = null,
        Category? category = Category.Office,
        IReadOnlyList<UserTask>? priorities = null,
        int? ramGb = null,
        int? storageGb = null)
        => new(budget, color, null, storageType, coolingType, category, priorities, ramGb, storageGb);

    [Fact]
    public void Validate_NullRequest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => BuildRequestValidator.Validate(null!));
    }

    [Fact]
    public void Validate_AllFieldsAtLimits_DoesNotThrow()
    {
        var request = Request(
            budget: BuildRequestLimits.MaxBudget,
            color: new string('a', BuildRequestLimits.MaxColorLength),
            storageType: StorageType.SSD,
            coolingType: CoolingType.Air,
            priorities: [UserTask.Gaming4K, UserTask.Programming],
            ramGb: BuildRequestLimits.MaxRamGb,
            storageGb: BuildRequestLimits.MaxStorageGb);

        var exception = Record.Exception(() => BuildRequestValidator.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_EmptyOptionalFields_DoesNotThrow()
    {
        var exception = Record.Exception(() => BuildRequestValidator.Validate(Request(category: null)));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_BudgetAboveMax_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(budget: BuildRequestLimits.MaxBudget + 1)));
    }

    [Fact]
    public void Validate_UndefinedCategory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(category: (Category)999)));
    }

    [Fact]
    public void Validate_UndefinedStorageType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(storageType: (StorageType)999)));
    }

    [Fact]
    public void Validate_UndefinedCoolingType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(coolingType: (CoolingType)999)));
    }

    [Fact]
    public void Validate_UndefinedPriority_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(priorities: [UserTask.OfficeWork, (UserTask)999])));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-8)]
    [InlineData(BuildRequestLimits.MaxRamGb + 1)]
    public void Validate_RamOutOfRange_Throws(int ramGb)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(ramGb: ramGb)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    [InlineData(BuildRequestLimits.MaxStorageGb + 1)]
    public void Validate_StorageOutOfRange_Throws(int storageGb)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(storageGb: storageGb)));
    }

    [Fact]
    public void Validate_ColorTooLong_Throws()
    {
        var color = new string('a', BuildRequestLimits.MaxColorLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildRequestValidator.Validate(Request(color: color)));
    }

    [Fact]
    public async Task GenerateBuildAsync_InvalidRequest_ThrowsBeforeQueryingRepositories()
    {
        var engine = EngineFactory.Create();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => engine.GenerateBuildAsync(Request(budget: BuildRequestLimits.MaxBudget + 1)));
    }
}
