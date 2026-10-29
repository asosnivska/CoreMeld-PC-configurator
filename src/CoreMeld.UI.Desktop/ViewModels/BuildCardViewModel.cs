using System.Globalization;
using System.Windows.Input;
using CoreMeld.Domain.Models;

namespace CoreMeld.UI.Desktop.ViewModels;

public class BuildCardViewModel : ViewModelBase
{
    private static readonly CultureInfo Ua = new("uk-UA");

    public BuildCardViewModel(BuildSummary summary, ICommand openCommand)
    {
        Id = summary.Id;
        Name = summary.Name;
        Price = summary.TotalPrice;
        PriceText = $"{summary.TotalPrice.ToString("N0", Ua)} грн";
        DateText = summary.CreatedAt.ToString("dd.MM.yyyy HH:mm");
        OpenCommand = openCommand;
    }

    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public string PriceText { get; }
    public string DateText { get; }
    public ICommand OpenCommand { get; }
}
