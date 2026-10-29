using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public static class ComponentMapper
{
    public static string SocketName(int socketId) => socketId switch
    {
        1 => "s-AM4",
        2 => "s-AM5",
        3 => "s-1700",
        _ => $"Сокет #{socketId}"
    };

    public static string FormFactorName(int formFactorId) => formFactorId switch
    {
        1 => "ATX",
        2 => "micro-ATX",
        3 => "mini-ITX",
        _ => $"Форм-фактор #{formFactorId}"
    };

    public static ComponentDto ToDto(Cpu cpu) => new(
        cpu.Id,
        ComponentCategory.Cpu,
        cpu.Name,
        cpu.Brand,
        cpu.Price,
        cpu.ImageUrl,
        [
            new("Сокет", SocketName(cpu.SocketId)),
            new("Кількість ядер", cpu.Cores.ToString()),
            new("Кількість потоків", cpu.Threads.ToString()),
            new("Базова частота", $"{cpu.BaseClock} ГГц"),
            new("Максимальна частота", $"{cpu.BoostClock} ГГц"),
            new("TDP", $"{cpu.TdpWatts} Вт"),
            new("Вбудована графіка", cpu.HasIntegratedGpu ? "Є" : "Немає")
        ]);

    public static ComponentDto ToDto(Motherboard mb) => new(
        mb.Id,
        ComponentCategory.Motherboard,
        mb.Name,
        mb.Brand,
        mb.Price,
        mb.ImageUrl,
        [
            new("Сокет", SocketName(mb.SocketId)),
            new("Чіпсет", mb.Chipset),
            new("Форм-фактор", FormFactorName(mb.FormFactorId)),
            new("Тип пам'яті", mb.MemoryType.ToString()),
            new("Слоти пам'яті", $"{mb.MemorySlots} шт."),
            new("Макс. об'єм ОЗП", $"{mb.MaxMemoryGb} ГБ"),
            new("Wi-Fi", mb.HasWifi ? "Є" : "Немає")
        ]);

    public static ComponentDto ToDto(Gpu gpu) => new(
        gpu.Id,
        ComponentCategory.Gpu,
        gpu.Name,
        gpu.Brand,
        gpu.Price,
        gpu.ImageUrl,
        [
            new("Об'єм пам'яті", $"{gpu.VramGb} ГБ"),
            new("Тип пам'яті", gpu.MemoryType.ToString()),
            new("Споживання (TDP)", $"{gpu.TdpWatts} Вт"),
            new("Довжина", $"{gpu.LengthMm} мм"),
            new("Рекомендований БЖ", $"{gpu.RecommendedPsuWatts} Вт")
        ]);

    public static ComponentDto ToDto(Ram ram) => new(
        ram.Id,
        ComponentCategory.Ram,
        ram.Name,
        ram.Brand,
        ram.Price,
        ram.ImageUrl,
        [
            new("Тип пам'яті", ram.RamMemoryType),
            new("Об'єм", $"{ram.CapacityGb} ГБ"),
            new("Кількість модулів", $"{ram.ModuleCount} шт."),
            new("Частота", $"{ram.FrequencyMhz} МГц")
        ]);

    public static ComponentDto ToDto(Storage storage) => new(
        storage.Id,
        ComponentCategory.Storage,
        storage.Name,
        storage.Brand,
        storage.Price,
        storage.ImageUrl,
        [
            new("Тип накопичувача", storage.DriveType),
            new("Інтерфейс", storage.InterfaceType),
            new("Об'єм", $"{storage.CapacityGb} ГБ")
        ]);

    public static ComponentDto ToDto(CpuCooler cooler) => new(
        cooler.Id,
        ComponentCategory.CpuCooler,
        cooler.Name,
        cooler.Brand,
        cooler.Price,
        cooler.ImageUrl,
        [
            new("Сокет", SocketName(cooler.SocketId)),
            new("Висота кулера", $"{cooler.HeightMm} мм"),
            new("Розсіювана потужність (TDP)", $"{cooler.TdpRatingWatts} Вт"),
            new("Рівень шуму", cooler.NoiseLevelDb.HasValue ? $"{cooler.NoiseLevelDb} дБ" : "н/д"),
            new("Швидкість обертання", cooler.FanRpm.HasValue ? $"{cooler.FanRpm} об/хв" : "н/д")
        ]);

    public static ComponentDto ToDto(PCCase pcCase) => new(
        pcCase.Id,
        ComponentCategory.Case,
        pcCase.Name,
        pcCase.Brand,
        pcCase.Price,
        pcCase.ImageUrl,
        [
            new("Макс. довжина відеокарти", $"{pcCase.MaxGpuLengthMm} мм"),
            new("Макс. висота кулера", $"{pcCase.MaxCpuCoolerHeightMm} мм")
        ]);

    public static ComponentDto ToDto(PowerSupply psu) => new(
        psu.Id,
        ComponentCategory.PowerSupply,
        psu.Name,
        psu.Brand,
        psu.Price,
        psu.ImageUrl,
        [
            new("Потужність", $"{psu.Wattage} Вт"),
            new("Сертифікат енергоефективності", psu.EfficiencyRating),
            new("Форм-фактор", FormFactorName(psu.FormFactorId))
        ]);
}

