using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;
using CoreMeld.UI.Desktop.Commands;
using System.Globalization;
using System.Windows;

namespace CoreMeld.UI.Desktop.ViewModels;

public class ResultViewModel : ViewModelBase
{
    private const double BarInnerWidth = 583;
    private const string UnusedColor = "#E3E3E3";

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

        BudgetSegments = BuildSegments(Items, budget);


        BackCommand = new RelayCommand(_ => onBack());
        ReplaceCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is ComponentCategory category)
                    onReplace(category);
            });

        HighlightCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is not ComponentCategory category)
                    return;

                foreach (var item in Items)
                    item.IsHighlighted = item.Category == category && !item.IsHighlighted;
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
    public RelayCommand HighlightCommand { get; }

    private static List<BudgetSegmentViewModel> BuildSegments(
        IReadOnlyList<ResultItemViewModel> items, decimal budget)
    {
        var total = items.Sum(i => i.Price);
        var scale = Math.Max(budget, total);
        if (scale <= 0)
            return [];

        var parts = items
            .Select(i => (Category: (ComponentCategory?)i.Category,
                          Color: i.Color,
                          Width: (double)(i.Price / scale) * BarInnerWidth,
                          Tip: $"{i.CategoryName}: {i.Name}, {i.PriceText}"))
            .ToList();

        var unused = budget - total;
        if (unused > 0)
        {
            parts.Add((null, UnusedColor,
                (double)(unused / scale) * BarInnerWidth,
                $"Невикористаний бюджет: {unused.ToString("N0", Ua)} грн"));
        }

        parts = parts.Where(p => p.Width > 0).ToList();

        const double r = 5;
        var result = new List<BudgetSegmentViewModel>();
        for (var i = 0; i < parts.Count; i++)
        {
            var isFirst = i == 0;
            var isLast = i == parts.Count - 1;
            var radius = new CornerRadius(
                isFirst ? r : 0,
                isLast ? r : 0,
                isLast ? r : 0,
                isFirst ? r : 0);

            result.Add(new BudgetSegmentViewModel(
                parts[i].Category, parts[i].Color, parts[i].Width, parts[i].Tip, radius));
        }

        return result;
    }
}
