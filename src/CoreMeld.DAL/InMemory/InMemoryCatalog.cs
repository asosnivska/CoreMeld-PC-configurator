using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

/// <summary>
/// Тимчасовий каталог всіх деталей у пам'яті. Заглушка, поки в БД немає реальних даних
/// Вмикається через USE_FAKE_DATA=true.
/// Сокети: 1 = AM4, 2 = AM5, 3 = LGA1700
/// Форм-фактори: 1 = ATX, 2 = Micro-ATX, 3 = Mini-ITX
/// </summary>
public static class InMemoryCatalog
{
    public static IReadOnlyList<Cpu> Cpus { get; } = new List<Cpu>
    {
        new() { Id = 101, CategoryId = (int)ComponentCategory.Cpu, Name = "AMD Ryzen 5 5600", Brand = "AMD", Color = "Silver", Price = 5200m, SocketId = 1, Cores = 6, Threads = 12, BaseClock = 3.5m, BoostClock = 4.4m, TdpWatts = 65, HasIntegratedGpu = false },
        new() { Id = 102, CategoryId = (int)ComponentCategory.Cpu, Name = "AMD Ryzen 5 7600", Brand = "AMD", Color = "Silver", Price = 8900m, SocketId = 2, Cores = 6, Threads = 12, BaseClock = 3.8m, BoostClock = 5.1m, TdpWatts = 65, HasIntegratedGpu = true },
        new() { Id = 103, CategoryId = (int)ComponentCategory.Cpu, Name = "Intel Core i5-12400F", Brand = "Intel", Color = "Silver", Price = 6000m, SocketId = 3, Cores = 6, Threads = 12, BaseClock = 2.5m, BoostClock = 4.4m, TdpWatts = 65, HasIntegratedGpu = false },
        new() { Id = 104, CategoryId = (int)ComponentCategory.Cpu, Name = "Intel Core i7-13700K", Brand = "Intel", Color = "Silver", Price = 15500m, SocketId = 3, Cores = 16, Threads = 24, BaseClock = 3.4m, BoostClock = 5.4m, TdpWatts = 125, HasIntegratedGpu = true },
    };

    public static IReadOnlyList<Motherboard> Motherboards { get; } = new List<Motherboard>
    {
        new() { Id = 201, CategoryId = (int)ComponentCategory.Motherboard, Name = "ASUS TUF Gaming A520M-Plus II", Brand = "ASUS", Color = "Black", Price = 3199m, SocketId = 1, FormFactorId = 2, Chipset = "A520", MemoryType = RamMemoryType.DDR4, MemorySlots = 4, MaxMemoryGb = 128, HasWifi = false },
        new() { Id = 202, CategoryId = (int)ComponentCategory.Motherboard, Name = "Gigabyte B650M DS3H", Brand = "Gigabyte", Color = "Black", Price = 5400m, SocketId = 2, FormFactorId = 2, Chipset = "B650", MemoryType = RamMemoryType.DDR5, MemorySlots = 4, MaxMemoryGb = 192, HasWifi = false },
        new() { Id = 203, CategoryId = (int)ComponentCategory.Motherboard, Name = "MSI PRO B660M-A WIFI DDR4", Brand = "MSI", Color = "Black", Price = 5600m, SocketId = 3, FormFactorId = 2, Chipset = "B660", MemoryType = RamMemoryType.DDR4, MemorySlots = 4, MaxMemoryGb = 128, HasWifi = true },
        new() { Id = 204, CategoryId = (int)ComponentCategory.Motherboard, Name = "ASUS PRIME Z790-P WIFI", Brand = "ASUS", Color = "Black", Price = 9800m, SocketId = 3, FormFactorId = 1, Chipset = "Z790", MemoryType = RamMemoryType.DDR5, MemorySlots = 4, MaxMemoryGb = 192, HasWifi = true },
    };

    public static IReadOnlyList<Ram> Rams { get; } = new List<Ram>
    {
        new() { Id = 301, CategoryId = (int)ComponentCategory.Ram, Name = "Kingston Fury Beast 16GB (2x8) DDR4-3200", Brand = "Kingston", Color = "Black", Price = 1600m, RamMemoryType = "DDR4", CapacityGb = 16, ModuleCount = 2, FrequencyMhz = 3200 },
        new() { Id = 302, CategoryId = (int)ComponentCategory.Ram, Name = "G.Skill Ripjaws V 32GB (2x16) DDR4-3600", Brand = "G.Skill", Color = "Black", Price = 3300m, RamMemoryType = "DDR4", CapacityGb = 32, ModuleCount = 2, FrequencyMhz = 3600 },
        new() { Id = 303, CategoryId = (int)ComponentCategory.Ram, Name = "Kingston Fury Beast 32GB (2x16) DDR5-5600", Brand = "Kingston", Color = "Black", Price = 3900m, RamMemoryType = "DDR5", CapacityGb = 32, ModuleCount = 2, FrequencyMhz = 5600 },
    };

    public static IReadOnlyList<Gpu> Gpus { get; } = new List<Gpu>
    {
        new() { Id = 401, CategoryId = (int)ComponentCategory.Gpu, Name = "MSI GeForce GTX 1650 D6 Ventus XS 4GB", Brand = "MSI", Color = "Black", Price = 6200m, VramGb = 4, TdpWatts = 75, LengthMm = 204, RecommendedPsuWatts = 300, MemoryType = VideoMemoryType.GDDR6 },
        new() { Id = 402, CategoryId = (int)ComponentCategory.Gpu, Name = "ASUS GeForce RTX 4060 Dual 8GB", Brand = "ASUS", Color = "Black", Price = 13500m, VramGb = 8, TdpWatts = 115, LengthMm = 227, RecommendedPsuWatts = 550, MemoryType = VideoMemoryType.GDDR6 },
        new() { Id = 403, CategoryId = (int)ComponentCategory.Gpu, Name = "Gigabyte GeForce RTX 4070 SUPER WINDFORCE OC 12GB", Brand = "Gigabyte", Color = "Black", Price = 28500m, VramGb = 12, TdpWatts = 220, LengthMm = 261, RecommendedPsuWatts = 650, MemoryType = VideoMemoryType.GDDR6X },
    };

    public static IReadOnlyList<Storage> Storages { get; } = new List<Storage>
    {
        new() { Id = 501, CategoryId = (int)ComponentCategory.Storage, Name = "Kingston NV2 500GB M.2 NVMe SSD", Brand = "Kingston", Color = "Black", Price = 1500m, DriveType = "SSD", InterfaceType = "NVMe", CapacityGb = 500 },
        new() { Id = 502, CategoryId = (int)ComponentCategory.Storage, Name = "Samsung 980 1TB M.2 NVMe SSD", Brand = "Samsung", Color = "Black", Price = 3100m, DriveType = "SSD", InterfaceType = "NVMe", CapacityGb = 1000 },
        new() { Id = 503, CategoryId = (int)ComponentCategory.Storage, Name = "Seagate BarraCuda 2TB HDD", Brand = "Seagate", Color = "Silver", Price = 2000m, DriveType = "HDD", InterfaceType = "SATA", CapacityGb = 2000 },
    };

    public static IReadOnlyList<CpuCooler> CpuCoolers { get; } = new List<CpuCooler>
    {
        new() { Id = 601, CategoryId = (int)ComponentCategory.CpuCooler, Name = "ID-Cooling SE-214-XT (AM4)", Brand = "ID-Cooling", Color = "Black", Price = 849m, HeightMm = 155, TdpRatingWatts = 180, FanRpm = 1500, NoiseLevelDb = 28.0m, SocketId = 1 },
        new() { Id = 602, CategoryId = (int)ComponentCategory.CpuCooler, Name = "DeepCool AK400 (AM5)", Brand = "DeepCool", Color = "Black", Price = 1200m, HeightMm = 155, TdpRatingWatts = 220, FanRpm = 1850, NoiseLevelDb = 29.0m, SocketId = 2 },
        new() { Id = 603, CategoryId = (int)ComponentCategory.CpuCooler, Name = "Thermalright Peerless Assassin 120 SE (LGA1700)", Brand = "Thermalright", Color = "Black", Price = 1500m, HeightMm = 155, TdpRatingWatts = 245, FanRpm = 1550, NoiseLevelDb = 25.6m, SocketId = 3 },
    };

    public static IReadOnlyList<PCCase> Cases { get; } = new List<PCCase>
    {
        new() { Id = 701, CategoryId = (int)ComponentCategory.Case, Name = "Gamemax Destroyer MB", Brand = "Gamemax", Color = "Black", Price = 1755m, MaxGpuLengthMm = 300, MaxCpuCoolerHeightMm = 155 },
        new() { Id = 702, CategoryId = (int)ComponentCategory.Case, Name = "DeepCool CC560", Brand = "DeepCool", Color = "Black", Price = 2300m, MaxGpuLengthMm = 370, MaxCpuCoolerHeightMm = 163 },
    };

    public static IReadOnlyList<PowerSupply> PowerSupplies { get; } = new List<PowerSupply>
    {
        new() { Id = 801, CategoryId = (int)ComponentCategory.PowerSupply, Name = "Chieftec Polaris 3.0 550W", Brand = "Chieftec", Color = "Black", Price = 1900m, FormFactorId = 1, Wattage = 550, EfficiencyRating = "80+ Bronze" },
        new() { Id = 802, CategoryId = (int)ComponentCategory.PowerSupply, Name = "Chieftec PPG-850-C", Brand = "Chieftec", Color = "Black", Price = 4769m, FormFactorId = 1, Wattage = 850, EfficiencyRating = "80+ Gold" },
    };

    public static IReadOnlyList<Component> All { get; } =
    [
        .. Cpus, .. Motherboards, .. Rams, .. Gpus, .. Storages, .. CpuCoolers, .. Cases, .. PowerSupplies
    ];
}