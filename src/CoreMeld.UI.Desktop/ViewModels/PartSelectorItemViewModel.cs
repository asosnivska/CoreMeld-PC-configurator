using System.Globalization;
using CoreMeld.BLL.DTO;

namespace CoreMeld.UI.Desktop.ViewModels;

public class PartSelectorItemViewModel : ViewModelBase
{
    private static readonly CultureInfo Ua = new("uk-UA");
    private bool _isExpanded;

    public PartSelectorItemViewModel(ComponentDto component, bool isCurrent = false)
    {
        Component = component;
        Name = component.Name;
        Price = component.Price;
        PriceText = $"{component.Price.ToString("N0", Ua)} грн";
        Characteristics = component.Characteristics;
        IsCurrent = isCurrent;
    }

    public ComponentDto Component { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string PriceText { get; }
    public IReadOnlyList<CharacteristicDto> Characteristics { get; }
    public bool IsCurrent { get; set; }

    public bool ShowSelectButton => !IsCurrent;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
}
