using System.Globalization;
using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class ResultViewModel : ViewModelBase
{
    private const double BarInnerWidth = 583;

    private static readonly CultureInfo Ua = new("uk-UA");

    public ResultViewModel(
        PCBuildDto build,
        decimal budget,
        Action onBack,
        Action<ComponentCategory> onReplace)
    {
        Budget = budget;
        TotalPrice = build.TotalPrice;

        Items = build.Components
            .OrderBy(c => c.Category)
            .Select(c => new ResultItemViewModel(c.Category, c.Name, c.Price))
            .ToList();

        BudgetSegments = Items
            .Select(i => new BudgetSegmentViewModel(
                i.Color,
                budget > 0 ? (double)(i.Price / budget) * BarInnerWidth : 0))
            .ToList();

        BackCommand = new RelayCommand(_ => onBack());
        ReplaceCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is ComponentCategory category)
                    onReplace(category);
            });
    }

    public IReadOnlyList<ResultItemViewModel> Items { get; }
    public IReadOnlyList<BudgetSegmentViewModel> BudgetSegments { get; }
    public decimal TotalPrice { get; }
    public decimal Budget { get; }

    public string BudgetCaption =>
        $"{TotalPrice.ToString("N0", Ua)} / {Budget.ToString("N0", Ua)} UAH";

    public RelayCommand BackCommand { get; }
    public RelayCommand ReplaceCommand { get; }
}
