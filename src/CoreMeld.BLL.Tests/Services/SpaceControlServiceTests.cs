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
    
        [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, true)]
    [InlineData(1, 2, false)]
    [InlineData(2, 3, false)]
    [InlineData(3, 3, true)]
    public void FitsBoard_BoardAgainstCaseFormFactor_ReturnsExpected(int boardFormFactorId, int caseFormFactorId, bool expected)
    {
        var service = CreateService();

        var result = service.FitsBoard(CreateCase(maxBoardFormFactorId: caseFormFactorId), CreateBoard(boardFormFactorId));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FitsBoard_CaseFormFactorMissing_Throws()
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(
            () => service.FitsBoard(CreateCase(maxBoardFormFactorId: null), CreateBoard()));
    }

    [Fact]
    public void FitsBoard_NullCase_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsBoard(null!, CreateBoard()));
    }

    [Fact]
    public void FitsBoard_NullMotherboard_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsBoard(CreateCase(), null!));
    }

    [Theory]
    [InlineData(300, true)]
    [InlineData(350, true)]
    [InlineData(351, false)]
    public void FitsGpu_LengthAgainstLimit_ReturnsExpected(int gpuLength, bool expected)
    {
        var service = CreateService();

        var result = service.FitsGpu(CreateCase(maxGpu: 350), CreateGpu(gpuLength));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FitsGpu_NullCase_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsGpu(null!, CreateGpu()));
    }

    [Fact]
    public void FitsGpu_NullGpu_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsGpu(CreateCase(), null!));
    }

    [Theory]
    [InlineData(150, true)]
    [InlineData(160, true)]
    [InlineData(161, false)]
    public void FitsCooler_HeightAgainstLimit_ReturnsExpected(int coolerHeight, bool expected)
    {
        var service = CreateService();

        var result = service.FitsCooler(CreateCase(maxCooler: 160), CreateCooler(coolerHeight));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FitsCooler_NullCase_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsCooler(null!, CreateCooler()));
    }

    [Fact]
    public void FitsCooler_NullCooler_Throws()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.FitsCooler(CreateCase(), null!));
    }
}