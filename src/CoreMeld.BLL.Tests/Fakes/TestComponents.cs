using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Fakes;

public static class TestComponents
{
    public static Cpu CreateCpu(int id = 1, int socketId = 1, int tdp = 65, decimal price = 5000m)
        => new() { Id = id, Name = $"Cpu {id}", SocketId = socketId, TdpWatts = tdp, Price = price };

    public static Motherboard CreateMotherboard(int id = 2, int socketId = 1, int formFactorId = 1,
        RamMemoryType memoryType = RamMemoryType.DDR4, decimal price = 3000m)
        => new()
        {
            Id = id,
            Name = $"Board {id}",
            SocketId = socketId,
            FormFactorId = formFactorId,
            MemoryType = memoryType,
            MemorySlots = 4,
            MaxMemoryGb = 128,
            Price = price
        };

    public static Ram CreateRam(int id = 3, RamMemoryType type = RamMemoryType.DDR4, int modules = 2,
        int capacityGb = 16, decimal price = 1500m)
        => new()
        {
            Id = id,
            Name = $"Ram {id}",
            RamMemoryType = type,
            ModuleCount = modules,
            CapacityGb = capacityGb,
            Price = price
        };

    public static Storage CreateStorage(int id = 4, decimal price = 1000m)
        => new() { Id = id, Name = $"Storage {id}", DriveType = "SSD", CapacityGb = 500, Price = price };

    public static CpuCooler CreateCooler(int id = 5, int socketId = 1, int tdpRating = 150, int height = 150,
        decimal price = 800m)
        => new()
        {
            Id = id,
            Name = $"Cooler {id}",
            SocketId = socketId,
            TdpRatingWatts = tdpRating,
            HeightMm = height,
            Price = price
        };

    public static PCCase CreateCase(int id = 6, int? maxBoardFormFactorId = 1, int maxGpu = 350,
        int maxCooler = 160, decimal price = 2000m)
        => new()
        {
            Id = id,
            Name = $"Case {id}",
            MaxBoardFormFactorId = maxBoardFormFactorId,
            MaxGpuLengthMm = maxGpu,
            MaxCpuCoolerHeightMm = maxCooler,
            Price = price
        };

    public static PowerSupply CreatePsu(int id = 7, int wattage = 650, int? pcie8Pin = 2, bool? has12Vhpwr = false,
        decimal price = 2500m)
        => new()
        {
            Id = id,
            Name = $"Psu {id}",
            Wattage = wattage,
            Pcie8PinCount = pcie8Pin,
            Has12Vhpwr = has12Vhpwr,
            Price = price
        };

    public static Gpu CreateGpu(int id = 8, int tdp = 200, int length = 300,
        PowerConnectorType? connectorType = PowerConnectorType.Pin8, int? connectorCount = 1,
        decimal price = 12000m)
        => new()
        {
            Id = id,
            Name = $"Gpu {id}",
            TdpWatts = tdp,
            LengthMm = length,
            PowerConnectorType = connectorType,
            PowerConnectorCount = connectorCount,
            Price = price
        };

    public static PCBuild CreateBuild(bool withGpu = true, PowerSupply? psu = null)
        => new()
        {
            Cpu = CreateCpu(),
            Motherboard = CreateMotherboard(),
            Ram = CreateRam(),
            Storage = CreateStorage(),
            CpuCooler = CreateCooler(),
            Case = CreateCase(),
            PowerSupply = psu ?? CreatePsu(),
            Gpu = withGpu ? CreateGpu() : null
        };

    public static int[] Ids(IEnumerable<Component> components)
        => components.Select(c => c.Id).ToArray();
}