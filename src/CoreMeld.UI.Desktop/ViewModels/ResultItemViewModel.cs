using System.Globalization;
using CoreMeld.Domain.Models;

namespace CoreMeld.UI.Desktop.ViewModels;

public class ResultItemViewModel
{
    private static readonly CultureInfo Ua = new("uk-UA");

    public ResultItemViewModel(ComponentCategory category, string name, decimal price)
    {
        Category = category;
        Name = name;
        Price = price;
    }

    public ComponentCategory Category { get; }
    public string Name { get; }
    public decimal Price { get; }

    public string CategoryName => Category switch
    {
        ComponentCategory.Cpu => "Процесор",
        ComponentCategory.Motherboard => "Материнська плата",
        ComponentCategory.Ram => "Оперативна пам'ять",
        ComponentCategory.Gpu => "Відеокарта",
        ComponentCategory.Storage => "Пристрій зберігання даних",
        ComponentCategory.CpuCooler => "Охолодження процесору",
        ComponentCategory.Case => "Корпус",
        ComponentCategory.PowerSupply => "Блок живлення",
        _ => Category.ToString()
    };

    public string Color => Category switch
    {
        ComponentCategory.Cpu => "#4285F4",
        ComponentCategory.Motherboard => "#EA4335",
        ComponentCategory.Ram => "#FCD04F",
        ComponentCategory.Gpu => "#05C274",
        ComponentCategory.Storage => "#FF6D01",
        ComponentCategory.CpuCooler => "#46BDC6",
        ComponentCategory.Case => "#F07B72",
        ComponentCategory.PowerSupply => "#4285F4",
        _ => "#999999"
    };

    public string PriceText => $"{Price.ToString("N0", Ua)} грн";
}