using CoreMeld.BLL.Services;
using CoreMeld.BLL.Tests.Fakes;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;
using static CoreMeld.BLL.Tests.Fakes.TestComponents;

namespace CoreMeld.BLL.Tests.Services;

public class ConfiguratorEngineReplaceTests
{
    private static ConfiguratorEngine CreateEngine() => EngineFactory.Create();

    [Fact]
    public void ReplaceComponent_CompatibleGpu_RecalculatesPriceAndTdp()
    {
        var engine = CreateEngine();
        var newGpu = CreateGpu(30, tdp: 250, price: 20000m);

        var result = engine.ReplaceComponent(CreateBuild(), ComponentCategory.Gpu, newGpu);

        Assert.Equal(35800m, result.TotalPrice);
        Assert.Equal(415, result.TotalTdp);
        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void ReplaceComponent_CpuWithOtherSocket_ReturnsNotCompatibleWithoutThrowing()
    {
        var engine = CreateEngine();
        var newCpu = CreateCpu(30, socketId: 2, price: 7000m);

        var result = engine.ReplaceComponent(CreateBuild(), ComponentCategory.Cpu, newCpu);

        Assert.False(result.IsCompatible);
        Assert.Equal(29800m, result.TotalPrice);
    }

    [Fact]
    public void ReplaceComponent_GpuTooPowerfulForPsu_ReturnsNotCompatible()
    {
        var engine = CreateEngine();
        var newGpu = CreateGpu(30, tdp: 400);

        var result = engine.ReplaceComponent(CreateBuild(), ComponentCategory.Gpu, newGpu);

        Assert.False(result.IsCompatible);
        Assert.Equal(565, result.TotalTdp);
    }

    [Fact]
    public void ReplaceComponent_ReplacedComponentAppearsInResult()
    {
        var engine = CreateEngine();
        var newGpu = CreateGpu(30);

        var result = engine.ReplaceComponent(CreateBuild(), ComponentCategory.Gpu, newGpu);

        var gpuDto = Assert.Single(result.Components, c => c.Category == ComponentCategory.Gpu);
        Assert.Equal(30, gpuDto.Id);
    }

    [Fact]
    public void ReplaceComponent_BuildWithGpu_ReturnsComponentsInExpectedOrder()
    {
        var engine = CreateEngine();

        var result = engine.ReplaceComponent(CreateBuild(), ComponentCategory.Ram, CreateRam(30));

        var expected = new[]
        {
            ComponentCategory.Cpu,
            ComponentCategory.Motherboard,
            ComponentCategory.Gpu,
            ComponentCategory.Ram,
            ComponentCategory.Storage,
            ComponentCategory.CpuCooler,
            ComponentCategory.Case,
            ComponentCategory.PowerSupply
        };
        Assert.Equal(expected, result.Components.Select(c => c.Category).ToArray());
    }

    [Fact]
    public void ReplaceComponent_BuildWithoutGpu_AddingGpuIncludesItInResult()
    {
        var engine = CreateEngine();

        var result = engine.ReplaceComponent(CreateBuild(withGpu: false), ComponentCategory.Gpu, CreateGpu(30));

        Assert.Equal(8, result.Components.Count);
        Assert.Equal(365, result.TotalTdp);
        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void ReplaceComponent_DoesNotMutateInputBuild()
    {
        var engine = CreateEngine();
        var build = CreateBuild();
        var originalGpu = build.Gpu;

        engine.ReplaceComponent(build, ComponentCategory.Gpu, CreateGpu(30));

        Assert.Same(originalGpu, build.Gpu);
    }

    [Fact]
    public void ReplaceComponent_CategoryDoesNotMatchComponent_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentException>(
            () => engine.ReplaceComponent(CreateBuild(), ComponentCategory.Gpu, CreateCpu(30)));
    }

    [Fact]
    public void ReplaceComponent_UnsupportedComponentType_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentException>(
            () => engine.ReplaceComponent(CreateBuild(), ComponentCategory.Cpu, new Component()));
    }

    [Fact]
    public void ReplaceComponent_IncompleteBuild_Throws()
    {
        var engine = CreateEngine();
        var build = CreateBuild();
        build.Ram = null!;

        Assert.Throws<InvalidOperationException>(
            () => engine.ReplaceComponent(build, ComponentCategory.Gpu, CreateGpu(30)));
    }

    [Fact]
    public void ReplaceComponent_NullBuild_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentNullException>(
            () => engine.ReplaceComponent(null!, ComponentCategory.Gpu, CreateGpu(30)));
    }

    [Fact]
    public void ReplaceComponent_NullComponent_Throws()
    {
        var engine = CreateEngine();

        Assert.Throws<ArgumentNullException>(
            () => engine.ReplaceComponent(CreateBuild(), ComponentCategory.Gpu, null!));
    }
}