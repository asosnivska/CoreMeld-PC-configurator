using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Services;

public class ComponentDtoMapperTests
{
    private static string? GetValue(ComponentDto dto, string name)
        => dto.Characteristics.FirstOrDefault(c => c.Name == name)?.Value;

    public static TheoryData<Component, ComponentCategory> CategoryData => new()
    {
        { new Cpu(), ComponentCategory.Cpu },
        { new Motherboard(), ComponentCategory.Motherboard },
        { new Gpu(), ComponentCategory.Gpu },
        { new Ram(), ComponentCategory.Ram },
        { new Storage(), ComponentCategory.Storage },
        { new CpuCooler(), ComponentCategory.CpuCooler },
        { new PCCase(), ComponentCategory.Case },
        { new PowerSupply(), ComponentCategory.PowerSupply }
    };

    [Fact]
    public void ToDto_NullComponent_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ComponentDtoMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_UnsupportedComponentType_Throws()
    {
        Assert.Throws<ArgumentException>(() => ComponentDtoMapper.ToDto(new Component()));
    }

    [Theory]
    [MemberData(nameof(CategoryData))]
    public void ToDto_ComponentType_ReturnsMatchingCategory(Component component, ComponentCategory expected)
    {
        var dto = ComponentDtoMapper.ToDto(component);

        Assert.Equal(expected, dto.Category);
    }

    [Fact]
    public void ToDto_CommonFields_AreCopied()
    {
        var cpu = new Cpu { Id = 7, Name = "Ryzen 5 5600", Brand = "AMD", Price = 5200m };

        var dto = ComponentDtoMapper.ToDto(cpu);

        Assert.Equal(7, dto.Id);
        Assert.Equal("Ryzen 5 5600", dto.Name);
        Assert.Equal("AMD", dto.Brand);
        Assert.Equal(5200m, dto.Price);
    }

    [Fact]
    public void ToDto_EmptyImageUrl_ReturnsNull()
    {
        var dto = ComponentDtoMapper.ToDto(new Cpu { ImageUrl = "" });

        Assert.Null(dto.ImageUrl);
    }

    [Fact]
    public void ToDto_ImageUrlSet_KeepsUrl()
    {
        var dto = ComponentDtoMapper.ToDto(new Cpu { ImageUrl = "https://example.com/cpu.jpg" });

        Assert.Equal("https://example.com/cpu.jpg", dto.ImageUrl);
    }

    [Fact]
    public void ToDto_ColorSet_AddsColorCharacteristic()
    {
        var dto = ComponentDtoMapper.ToDto(new Cpu { Color = "Black" });

        Assert.Equal("Black", GetValue(dto, "Колір"));
    }

    [Fact]
    public void ToDto_ColorEmpty_DoesNotAddColorCharacteristic()
    {
        var dto = ComponentDtoMapper.ToDto(new Cpu { Color = "" });

        Assert.Null(GetValue(dto, "Колір"));
    }

    [Fact]
    public void ToDto_Cpu_ReturnsKeyCharacteristics()
    {
        var cpu = new Cpu { Cores = 8, Threads = 16, TdpWatts = 65 };

        var dto = ComponentDtoMapper.ToDto(cpu);

        Assert.Equal("8", GetValue(dto, "Ядра"));
        Assert.Equal("16", GetValue(dto, "Потоки"));
        Assert.Equal("65 Вт", GetValue(dto, "TDP"));
    }

    [Theory]
    [InlineData(true, "Так")]
    [InlineData(false, "Ні")]
    public void ToDto_CpuIntegratedGpu_ReturnsYesNo(bool hasIntegratedGpu, string expected)
    {
        var dto = ComponentDtoMapper.ToDto(new Cpu { HasIntegratedGpu = hasIntegratedGpu });

        Assert.Equal(expected, GetValue(dto, "Вбудована графіка"));
    }

    [Theory]
    [InlineData(true, "Так")]
    [InlineData(false, "Ні")]
    public void ToDto_MotherboardWifi_ReturnsYesNo(bool hasWifi, string expected)
    {
        var dto = ComponentDtoMapper.ToDto(new Motherboard { HasWifi = hasWifi });

        Assert.Equal(expected, GetValue(dto, "Wi-Fi"));
    }

    [Fact]
    public void ToDto_Motherboard_ReturnsKeyCharacteristics()
    {
        var board = new Motherboard { Chipset = "B550", MemorySlots = 4, MaxMemoryGb = 128 };

        var dto = ComponentDtoMapper.ToDto(board);

        Assert.Equal("B550", GetValue(dto, "Чипсет"));
        Assert.Equal("4", GetValue(dto, "Слоти ОЗП"));
        Assert.Equal("128 ГБ", GetValue(dto, "Макс. обсяг ОЗП"));
    }

    [Fact]
    public void ToDto_Gpu_ReturnsKeyCharacteristics()
    {
        var gpu = new Gpu { VramGb = 8, TdpWatts = 115, LengthMm = 227, RecommendedPsuWatts = 550 };

        var dto = ComponentDtoMapper.ToDto(gpu);

        Assert.Equal("8 ГБ", GetValue(dto, "Відеопам'ять"));
        Assert.Equal("115 Вт", GetValue(dto, "TDP"));
        Assert.Equal("227 мм", GetValue(dto, "Довжина"));
        Assert.Equal("550 Вт", GetValue(dto, "Рекомендований БЖ"));
    }

    [Theory]
    [InlineData(null, null, "Не вказано")]
    [InlineData(PowerConnectorType.None, 0, "Не потрібне")]
    [InlineData(PowerConnectorType.Pin6, 2, "2 × 6-pin")]
    [InlineData(PowerConnectorType.Pin8, 1, "1 × 8-pin")]
    [InlineData(PowerConnectorType.Hpwr12V, 1, "12VHPWR")]
    public void ToDto_GpuPowerConnector_ReturnsReadableText(
        PowerConnectorType? type, int? count, string expected)
    {
        var gpu = new Gpu { PowerConnectorType = type, PowerConnectorCount = count };

        var dto = ComponentDtoMapper.ToDto(gpu);

        Assert.Equal(expected, GetValue(dto, "Додаткове живлення"));
    }

    [Fact]
    public void ToDto_Ram_ReturnsKeyCharacteristics()
    {
        var ram = new Ram
        {
            RamMemoryType = RamMemoryType.DDR4,
            CapacityGb = 16,
            ModuleCount = 2,
            FrequencyMhz = 3200
        };

        var dto = ComponentDtoMapper.ToDto(ram);

        Assert.Equal("DDR4", GetValue(dto, "Тип"));
        Assert.Equal("16 ГБ", GetValue(dto, "Обсяг"));
        Assert.Equal("2", GetValue(dto, "Модулів"));
        Assert.Equal("3200 МГц", GetValue(dto, "Частота"));
    }

    [Fact]
    public void ToDto_Storage_ReturnsKeyCharacteristics()
    {
        var storage = new Storage { DriveType = "SSD", InterfaceType = "NVMe", CapacityGb = 1000 };

        var dto = ComponentDtoMapper.ToDto(storage);

        Assert.Equal("SSD", GetValue(dto, "Тип"));
        Assert.Equal("NVMe", GetValue(dto, "Інтерфейс"));
        Assert.Equal("1000 ГБ", GetValue(dto, "Обсяг"));
    }

    [Fact]
    public void ToDto_Cooler_ReturnsBaseCharacteristics()
    {
        var cooler = new CpuCooler { HeightMm = 155, TdpRatingWatts = 220 };

        var dto = ComponentDtoMapper.ToDto(cooler);

        Assert.Equal("155 мм", GetValue(dto, "Висота"));
        Assert.Equal("220 Вт", GetValue(dto, "Розсіювана потужність"));
    }

    [Fact]
    public void ToDto_CoolerWithoutOptionalFields_OmitsThem()
    {
        var cooler = new CpuCooler { FanRpm = null, NoiseLevelDb = null };

        var dto = ComponentDtoMapper.ToDto(cooler);

        Assert.Null(GetValue(dto, "Обертів вентилятора"));
        Assert.Null(GetValue(dto, "Рівень шуму"));
    }

    [Fact]
    public void ToDto_CoolerWithOptionalFields_IncludesThem()
    {
        var cooler = new CpuCooler { FanRpm = 1800, NoiseLevelDb = 25m };

        var dto = ComponentDtoMapper.ToDto(cooler);

        Assert.Equal("1800 об/хв", GetValue(dto, "Обертів вентилятора"));
        Assert.Equal("25 дБ", GetValue(dto, "Рівень шуму"));
    }

    [Fact]
    public void ToDto_Case_ReturnsKeyCharacteristics()
    {
        var pcCase = new PCCase { MaxGpuLengthMm = 350, MaxCpuCoolerHeightMm = 160 };

        var dto = ComponentDtoMapper.ToDto(pcCase);

        Assert.Equal("350 мм", GetValue(dto, "Макс. довжина відеокарти"));
        Assert.Equal("160 мм", GetValue(dto, "Макс. висота кулера"));
    }

    [Fact]
    public void ToDto_PowerSupply_ReturnsKeyCharacteristics()
    {
        var psu = new PowerSupply { Wattage = 650, EfficiencyRating = "80+ Bronze", Pcie8PinCount = 2 };

        var dto = ComponentDtoMapper.ToDto(psu);

        Assert.Equal("650 Вт", GetValue(dto, "Потужність"));
        Assert.Equal("80+ Bronze", GetValue(dto, "Сертифікат"));
        Assert.Equal("2", GetValue(dto, "PCIe 8-pin роз'ємів"));
    }

    [Fact]
    public void ToDto_PowerSupplyWithoutConnectorData_ReturnsNotSpecified()
    {
        var psu = new PowerSupply { Pcie8PinCount = null, Has12Vhpwr = null };

        var dto = ComponentDtoMapper.ToDto(psu);

        Assert.Equal("Не вказано", GetValue(dto, "PCIe 8-pin роз'ємів"));
        Assert.Equal("Не вказано", GetValue(dto, "12VHPWR"));
    }

    [Theory]
    [InlineData(true, "Так")]
    [InlineData(false, "Ні")]
    public void ToDto_PowerSupplyHpwr_ReturnsYesNo(bool has12Vhpwr, string expected)
    {
        var dto = ComponentDtoMapper.ToDto(new PowerSupply { Has12Vhpwr = has12Vhpwr });

        Assert.Equal(expected, GetValue(dto, "12VHPWR"));
    }
}