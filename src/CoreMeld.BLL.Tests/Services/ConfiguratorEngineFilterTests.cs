using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;
using static CoreMeld.BLL.Tests.Fakes.TestComponents;

namespace CoreMeld.BLL.Tests.Services;

public class ConfiguratorEngineFilterTests
{
    private static ConfiguratorEngine CreateEngine() => EngineFactory.Create();

    [Fact]
    public void FilterCompatibleComponents_EmptyBuild_ReturnsWholePool()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateCpu(10, socketId: 1), CreateCpu(11, socketId: 2) };

        var result = engine.FilterCompatibleComponents(pool, new PCBuild());

        Assert.Equal(new[] { 10, 11 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_CpuWithOtherSocket_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateCpu(10, socketId: 1), CreateCpu(11, socketId: 2) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_CpuHotterThanCoolerRating_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateCpu(10, tdp: 150), CreateCpu(11, tdp: 151) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_RamWithOtherMemoryType_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[]
        {
            CreateRam(10, RamMemoryType.DDR4),
            CreateRam(11, RamMemoryType.DDR5)
        };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_GpuLongerThanCaseLimit_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateGpu(10, length: 350), CreateGpu(11, length: 351) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_GpuNeedsMoreConnectorsThanPsuHas_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[]
        {
            CreateGpu(10, connectorCount: 2),
            CreateGpu(11, connectorCount: 3)
        };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void FilterCompatibleComponents_Gpu12Vhpwr_DependsOnPsuSupport(bool psuHas12Vhpwr, bool expectedIncluded)
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateGpu(10, connectorType: PowerConnectorType.Hpwr12V, connectorCount: 1) };
        var build = CreateBuild(psu: CreatePsu(has12Vhpwr: psuHas12Vhpwr));

        var result = engine.FilterCompatibleComponents(pool, build);

        Assert.Equal(expectedIncluded, result.Count == 1);
    }

    [Fact]
    public void FilterCompatibleComponents_GpuTooPowerfulForPsu_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateGpu(10, tdp: 200), CreateGpu(11, tdp: 400) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_PsuBelowRecommendedWattage_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreatePsu(10, wattage: 450), CreatePsu(11, wattage: 400) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_PsuWithoutConnectorData_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[]
        {
            CreatePsu(10),
            CreatePsu(11, pcie8Pin: null),
            CreatePsu(12, has12Vhpwr: null)
        };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_PsuWithoutGpuInBuild_ConnectorDataIsNotRequired()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreatePsu(10, pcie8Pin: null, has12Vhpwr: null) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild(withGpu: false));

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_CaseTooSmallForBoard_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[]
        {
            CreateCase(10, maxBoardFormFactorId: 1),
            CreateCase(11, maxBoardFormFactorId: 2)
        };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_CaseWithoutFormFactor_IsExcludedWithoutThrowing()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateCase(10, maxBoardFormFactorId: null) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Empty(result);
    }

    [Fact]
    public void FilterCompatibleComponents_CoolerTooTallForCase_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateCooler(10, height: 160), CreateCooler(11, height: 161) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_CoolerWithOtherSocketOrLowRating_IsExcluded()
    {
        var engine = CreateEngine();
        var pool = new Component[]
        {
            CreateCooler(10),
            CreateCooler(11, socketId: 2),
            CreateCooler(12, tdpRating: 64)
        };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_Storage_IsAlwaysIncluded()
    {
        var engine = CreateEngine();
        var pool = new Component[] { CreateStorage(10), CreateStorage(11) };

        var result = engine.FilterCompatibleComponents(pool, CreateBuild());

        Assert.Equal(new[] { 10, 11 }, Ids(result));
    }

    [Fact]
    public void FilterCompatibleComponents_DoesNotMutateBuild()
    {
        var engine = CreateEngine();
        var build = CreateBuild();
        var originalCpu = build.Cpu;
        var originalGpu = build.Gpu;
        var pool = new Component[] { CreateCpu(10), CreateGpu(11) };

        engine.FilterCompatibleComponents(pool, build);

        Assert.Same(originalCpu, build.Cpu);
        Assert.Same(originalGpu, build.Gpu);
    }

    [Fact]
    public void FilterCompatibleComponents_NullPool_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentNullException>(
            () => engine.FilterCompatibleComponents(null!, CreateBuild()));
    }

    [Fact]
    public void FilterCompatibleComponents_NullBuild_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentNullException>(
            () => engine.FilterCompatibleComponents(new Component[] { CreateCpu() }, null!));
    }
}