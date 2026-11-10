using System.Windows;
using CoreMeld.Domain.Enums;
namespace CoreMeld.UI.Desktop.ViewModels;

public class BudgetSegmentViewModel
{
    public BudgetSegmentViewModel(
        ComponentCategory? category,
        string color,
        double width,
        string toolTipText,
        CornerRadius cornerRadius)
    {
        Category = category;
        Color = color;
        Width = width;
        ToolTipText = toolTipText;
        CornerRadius = cornerRadius;
    }

    public ComponentCategory? Category { get; }
    public string Color { get; }
    public double Width { get; }
    public string ToolTipText { get; }
    public CornerRadius CornerRadius { get; }
}