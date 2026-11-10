using System.Globalization;
using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.UI.Desktop.ViewModels;

public class PartItemViewModel
{
    private static readonly CultureInfo Ua = new("uk-UA");

    public PartItemViewModel(Component component, ComponentDto dto, bool isCurrent)
    {
        Component = component;
        Name = dto.Name;
        Price = dto.Price;
        Characteristics = dto.Characteristics;
        IsCurrent = isCurrent;
    }

    public Component Component { get; }
    public string Name { get; }
    public decimal Price { get; }
    public IReadOnlyList<CharacteristicDto> Characteristics { get; }
    public bool IsCurrent { get; }

    public string PriceText => $"{Price.ToString("N0", Ua)} грн";
    public string ActionText => IsCurrent ? "Поточна" : "Обрати";
}