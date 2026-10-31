using CoreMeld.BLL.Services;
using CoreMeld.BLL.Settings;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Services;

public class SpaceControlServiceTests
{
    private static SpaceControlService CreateService()
    {
        var provider = new FakeFormFactorSpecsProvider(new Dictionary<int, BoardSize>
        {
            [1] = new BoardSize(305, 244),
            [2] = new BoardSize(244, 244),
            [3] = new BoardSize(170, 170)
        });

        return new SpaceControlService(provider);
    }

    private static PCCase CreateCase(int? maxBoardFormFactorId = 1, int maxGpu = 350, int maxCooler = 160)
        => new()
        {
            MaxBoardFormFactorId = maxBoardFormFactorId,
            MaxGpuLengthMm = maxGpu,
            MaxCpuCoolerHeightMm = maxCooler
        };

    private static Motherboard CreateBoard(int formFactorId = 1)
        => new() { FormFactorId = formFactorId };

    private static Gpu CreateGpu(int length = 300)
        => new() { LengthMm = length };

    private static CpuCooler CreateCooler(int height = 150)
        => new() { HeightMm = height };

    [Fact]
    public void FitsInCase_AllWithinLimits_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(), CreateCooler());

        Assert.True(result);
    }

    [Fact]
    public void FitsInCase_GpuLengthEqualsLimit_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(350), CreateCooler());

        Assert.True(result);
    }

    [Fact]
    public void FitsInCase_GpuLengthExceedsLimit_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(351), CreateCooler());

        Assert.False(result);
    }

    [Fact]
    public void FitsInCase_CoolerHeightEqualsLimit_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(), CreateCooler(160));

        Assert.True(result);
    }

    [Fact]
    public void FitsInCase_CoolerHeightExceedsLimit_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(), CreateCooler(161));

        Assert.False(result);
    }

    [Fact]
    public void FitsInCase_NoGpu_ReturnsTrue()
    {
        var service = CreateService();

        var result = service.FitsInCase(CreateCase(), CreateBoard(), null, CreateCooler());

        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, true)]
    [InlineData(3, 1, true)]
    [InlineData(1, 2, false)]
    [InlineData(2, 3, false)]
    [InlineData(3, 3, true)]
    public void FitsInCase_BoardAgainstCaseFormFactor_ReturnsExpected(int boardFormFactorId, int caseFormFactorId, bool expected)
    {
        var service = CreateService();

        var result = service.FitsInCase(
            CreateCase(maxBoardFormFactorId: caseFormFactorId),
            CreateBoard(boardFormFactorId),
            CreateGpu(),
            CreateCooler());

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FitsInCase_CaseFormFactorMissing_Throws()
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(
            () => service.FitsInCase(CreateCase(maxBoardFormFactorId: null), CreateBoard(), CreateGpu(), CreateCooler()));
    }

    [Fact]
    public void FitsInCase_UnknownBoardFormFactor_Throws()
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(
            () => service.FitsInCase(CreateCase(), CreateBoard(99), CreateGpu(), CreateCooler()));
    }

    [Fact]
    public void FitsInCase_NullCase_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(
            () => service.FitsInCase(null!, CreateBoard(), CreateGpu(), CreateCooler()));
    }

    [Fact]
    public void FitsInCase_NullMotherboard_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(
            () => service.FitsInCase(CreateCase(), null!, CreateGpu(), CreateCooler()));
    }

    [Fact]
    public void FitsInCase_NullCooler_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(
            () => service.FitsInCase(CreateCase(), CreateBoard(), CreateGpu(), null!));
    }
}