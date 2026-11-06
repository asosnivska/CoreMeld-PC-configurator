using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public static class ComponentDtoMapper
{
    public static ComponentDto ToDto(Component component)
    {
        ArgumentNullException.ThrowIfNull(component);
        
        var (category, characteristics) = component switch
        {
            Cpu cpu => (ComponentCategory.Cpu, CpuCharacteristics(cpu)),
            Motherboard board => (ComponentCategory.Motherboard, MotherboardCharacteristics(board)),
            Gpu gpu => (ComponentCategory.Gpu, GpuCharacteristics(gpu)),
            Ram ram => (ComponentCategory.Ram, RamCharacteristics(ram)),
            Storage storage => (ComponentCategory.Storage, StorageCharacteristics(storage)),
            CpuCooler cooler => (ComponentCategory.CpuCooler, CoolerCharacteristics(cooler)),
            PCCase pcCase => (ComponentCategory.Case, CaseCharacteristics(pcCase)),
            PowerSupply psu => (ComponentCategory.PowerSupply, PowerSupplyCharacteristics(psu)),
            _ => throw new ArgumentException(
                $"Непідтримуваний тип компонента: {component.GetType().Name}.",
                nameof(component))
        };
        var all = characteristics.ToList();

        if (!string.IsNullOrEmpty(component.Color))
        {
            all.Add(new("Колір", component.Color));
        }

        return new ComponentDto(
            component.Id,
            category,
            component.Name,
            component.Brand,
            component.Price,
            string.IsNullOrEmpty(component.ImageUrl) ? null : component.ImageUrl,
            all);
    }

    private static IReadOnlyList<CharacteristicDto> CpuCharacteristics(Cpu cpu)
    {
        return
        [
            new("Ядра", cpu.Cores.ToString()),
            new("Потоки", cpu.Threads.ToString()),
            new("Базова частота", $"{cpu.BaseClock} ГГц"),
            new("Boost-частота", $"{cpu.BoostClock} ГГц"),
            new("TDP", $"{cpu.TdpWatts} Вт"),
            new("Вбудована графіка", YesNo(cpu.HasIntegratedGpu))
        ];
    }

    private static IReadOnlyList<CharacteristicDto> MotherboardCharacteristics(Motherboard board)
    {
        return
        [
            new("Чипсет", board.Chipset),
            new("Тип пам'яті", board.MemoryType.ToString()),
            new("Слоти ОЗП", board.MemorySlots.ToString()),
            new("Макс. обсяг ОЗП", $"{board.MaxMemoryGb} ГБ"),
            new("Wi-Fi", YesNo(board.HasWifi))
        ];
    }

    private static IReadOnlyList<CharacteristicDto> GpuCharacteristics(Gpu gpu)
    {
        return
        [
            new("Відеопам'ять", $"{gpu.VramGb} ГБ"),
            new("Тип пам'яті", gpu.MemoryType.ToString()),
            new("TDP", $"{gpu.TdpWatts} Вт"),
            new("Довжина", $"{gpu.LengthMm} мм"),
            new("Рекомендований БЖ", $"{gpu.RecommendedPsuWatts} Вт"),
            new("Додаткове живлення", PowerConnectorText(gpu.PowerConnectorType, gpu.PowerConnectorCount))
        ];
    }
    
    private static string PowerConnectorText(PowerConnectorType? type, int? count)
    {
        if (type is null)
        {
            return "Не вказано";
        }

        return type.Value switch
        {
            PowerConnectorType.None => "Не потрібне",
            PowerConnectorType.Pin6 => $"{count} × 6-pin",
            PowerConnectorType.Pin8 => $"{count} × 8-pin",
            PowerConnectorType.Hpwr12V => "12VHPWR",
            _ => "Невідомо"
        };
    }

    private static IReadOnlyList<CharacteristicDto> RamCharacteristics(Ram ram)
    {
        return
        [
            new("Тип", ram.RamMemoryType.ToString()),
            new("Обсяг", $"{ram.CapacityGb} ГБ"),
            new("Модулів", ram.ModuleCount.ToString()),
            new("Частота", $"{ram.FrequencyMhz} МГц")
        ];
    }

    private static IReadOnlyList<CharacteristicDto> StorageCharacteristics(Storage storage)
    {
        return
        [
            new("Тип", storage.DriveType),
            new("Інтерфейс", storage.InterfaceType),
            new("Обсяг", $"{storage.CapacityGb} ГБ")
        ];
    }

    private static IReadOnlyList<CharacteristicDto> CoolerCharacteristics(CpuCooler cooler)
    {
        var list = new List<CharacteristicDto>
        {
            new("Висота", $"{cooler.HeightMm} мм"),
            new("Розсіювана потужність", $"{cooler.TdpRatingWatts} Вт")
        };

        if (cooler.FanRpm is not null)
        {
            list.Add(new("Обертів вентилятора", $"{cooler.FanRpm} об/хв"));
        }

        if (cooler.NoiseLevelDb is not null)
        {
            list.Add(new("Рівень шуму", $"{cooler.NoiseLevelDb} дБ"));
        }

        return list;
    }

    private static IReadOnlyList<CharacteristicDto> CaseCharacteristics(PCCase pcCase)
    {
        return
        [
            new("Макс. довжина відеокарти", $"{pcCase.MaxGpuLengthMm} мм"),
            new("Макс. висота кулера", $"{pcCase.MaxCpuCoolerHeightMm} мм")
        ];
    }

    private static IReadOnlyList<CharacteristicDto> PowerSupplyCharacteristics(PowerSupply psu)
    {
        return
        [
            new("Потужність", $"{psu.Wattage} Вт"),
            new("Сертифікат", psu.EfficiencyRating),
            new("PCIe 8-pin роз'ємів", psu.Pcie8PinCount?.ToString() ?? "Не вказано"),
            new("12VHPWR", psu.Has12Vhpwr is null ? "Не вказано" : YesNo(psu.Has12Vhpwr.Value))
        ];
    }
    
    private static string YesNo(bool b) => b ? "Так" : "Ні";
}
