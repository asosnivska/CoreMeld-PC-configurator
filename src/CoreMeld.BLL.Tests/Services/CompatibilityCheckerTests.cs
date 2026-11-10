using CoreMeld.BLL.Services;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Services;

public class CompatibilityCheckerTests
{
    private static CompatibilityChecker CreateChecker() => new CompatibilityChecker();
    private static Cpu CreateCpu(int socketId = 1) => new Cpu() { SocketId = socketId };

    private static Motherboard CreateMotherboard(int socketId = 1, RamMemoryType memoryType = RamMemoryType.DDR4,
        int slots = 4, int maxMemoryGb = 128) => new Motherboard()
        { SocketId = socketId, MemoryType = memoryType, MemorySlots = slots, MaxMemoryGb = maxMemoryGb};

    private static Ram CreateRam(RamMemoryType type = RamMemoryType.DDR4, int modules = 2, int capacityGb = 16)
        => new Ram() { RamMemoryType = type, ModuleCount = modules, CapacityGb = capacityGb };

    private static Gpu CreateGpu(PowerConnectorType? type = PowerConnectorType.Pin8, int? count = 1)
        => new Gpu() { PowerConnectorType = type, PowerConnectorCount = count };
    
    private static PowerSupply CreatePowerSupply(int? pcie8Pin = 2, bool? has12Vhpwr = false)
        =>new PowerSupply() { Pcie8PinCount = pcie8Pin, Has12Vhpwr = has12Vhpwr };
    
    private static CpuCooler CreateCooler(int socketId = 1, int tdpRating = 150)
        => new CpuCooler() { SocketId = socketId, TdpRatingWatts = tdpRating };
    
 [Fact]
    public void IsSocketCompatible_SameSocket_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsSocketCompatible(CreateCpu(1), CreateMotherboard(socketId: 1));

        Assert.True(result);
    }

    [Fact]
    public void IsSocketCompatible_DifferentSocket_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsSocketCompatible(CreateCpu(1), CreateMotherboard(socketId: 2));

        Assert.False(result);
    }

    [Fact]
    public void IsSocketCompatible_NullCpu_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsSocketCompatible(null!, CreateMotherboard()));
    }

    [Fact]
    public void IsSocketCompatible_NullMotherboard_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsSocketCompatible(CreateCpu(), null!));
    }

    [Fact]
    public void IsRamCompatible_AllMatch_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(CreateRam(), CreateMotherboard());

        Assert.True(result);
    }

    [Fact]
    public void IsRamCompatible_MemoryTypeMismatch_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(
            CreateRam(type: RamMemoryType.DDR5),
            CreateMotherboard(memoryType: RamMemoryType.DDR4));

        Assert.False(result);
    }

    [Fact]
    public void IsRamCompatible_ModulesEqualSlots_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(
            CreateRam(modules: 2),
            CreateMotherboard(slots: 2));

        Assert.True(result);
    }

    [Fact]
    public void IsRamCompatible_ModulesExceedSlots_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(
            CreateRam(modules: 3),
            CreateMotherboard(slots: 2));

        Assert.False(result);
    }

    [Fact]
    public void IsRamCompatible_CapacityEqualsMax_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(
            CreateRam(capacityGb: 32),
            CreateMotherboard(maxMemoryGb: 32));

        Assert.True(result);
    }

    [Fact]
    public void IsRamCompatible_CapacityExceedsMax_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsRamCompatible(
            CreateRam(capacityGb: 33),
            CreateMotherboard(maxMemoryGb: 32));

        Assert.False(result);
    }

    [Fact]
    public void IsRamCompatible_NullRam_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsRamCompatible(null!, CreateMotherboard()));
    }

    [Fact]
    public void IsRamCompatible_NullMotherboard_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsRamCompatible(CreateRam(), null!));
    }

    [Fact]
    public void IsPowerConnectorCompatible_NoGpu_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(null, CreatePowerSupply());

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_NoConnectorNeeded_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.None, 0),
            CreatePowerSupply(pcie8Pin: 0));

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Pin8EnoughConnectors_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Pin8, 1),
            CreatePowerSupply(pcie8Pin: 2));

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Pin8ConnectorsEqualRequired_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Pin8, 2),
            CreatePowerSupply(pcie8Pin: 2));

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Pin8NotEnoughConnectors_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Pin8, 3),
            CreatePowerSupply(pcie8Pin: 2));

        Assert.False(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Pin6EnoughConnectors_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Pin6, 1),
            CreatePowerSupply(pcie8Pin: 1));

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Hpwr12VWithoutSupport_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Hpwr12V, 1),
            CreatePowerSupply(has12Vhpwr: false));

        Assert.False(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_Hpwr12VWithSupport_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsPowerConnectorCompatible(
            CreateGpu(PowerConnectorType.Hpwr12V, 1),
            CreatePowerSupply(has12Vhpwr: true));

        Assert.True(result);
    }

    [Fact]
    public void IsPowerConnectorCompatible_GpuConnectorTypeMissing_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<InvalidOperationException>(
            () => checker.IsPowerConnectorCompatible(CreateGpu(type: null), CreatePowerSupply()));
    }

    [Fact]
    public void IsPowerConnectorCompatible_GpuConnectorCountMissing_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<InvalidOperationException>(
            () => checker.IsPowerConnectorCompatible(CreateGpu(count: null), CreatePowerSupply()));
    }

    [Fact]
    public void IsPowerConnectorCompatible_PsuPcieCountMissing_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<InvalidOperationException>(
            () => checker.IsPowerConnectorCompatible(
                CreateGpu(PowerConnectorType.Pin8, 1),
                CreatePowerSupply(pcie8Pin: null)));
    }

    [Fact]
    public void IsPowerConnectorCompatible_PsuHpwrFlagMissing_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<InvalidOperationException>(
            () => checker.IsPowerConnectorCompatible(
                CreateGpu(PowerConnectorType.Hpwr12V, 1),
                CreatePowerSupply(has12Vhpwr: null)));
    }

    [Fact]
    public void IsPowerConnectorCompatible_NullPowerSupply_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsPowerConnectorCompatible(CreateGpu(), null!));
    }
    
    [Fact]
    public void IsCoolerCompatible_SameSocketAndEnoughTdp_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsCoolerCompatible(
            new Cpu { SocketId = 1, TdpWatts = 65 },
            CreateCooler(socketId: 1, tdpRating: 150));

        Assert.True(result);
    }

    [Fact]
    public void IsCoolerCompatible_TdpRatingEqualsCpuTdp_ReturnsTrue()
    {
        var checker = CreateChecker();

        var result = checker.IsCoolerCompatible(
            new Cpu { SocketId = 1, TdpWatts = 150 },
            CreateCooler(tdpRating: 150));

        Assert.True(result);
    }

    [Fact]
    public void IsCoolerCompatible_TdpRatingBelowCpuTdp_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsCoolerCompatible(
            new Cpu { SocketId = 1, TdpWatts = 151 },
            CreateCooler(tdpRating: 150));

        Assert.False(result);
    }

    [Fact]
    public void IsCoolerCompatible_DifferentSocket_ReturnsFalse()
    {
        var checker = CreateChecker();

        var result = checker.IsCoolerCompatible(
            new Cpu { SocketId = 1, TdpWatts = 65 },
            CreateCooler(socketId: 2));

        Assert.False(result);
    }

    [Fact]
    public void IsCoolerCompatible_NullCpu_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(() => checker.IsCoolerCompatible(null!, CreateCooler()));
    }

    [Fact]
    public void IsCoolerCompatible_NullCooler_Throws()
    {
        var checker = CreateChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.IsCoolerCompatible(new Cpu { SocketId = 1, TdpWatts = 65 }, null!));
    }

}
