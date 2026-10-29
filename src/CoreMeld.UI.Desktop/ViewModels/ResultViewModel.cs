using System.Globalization;
using System.Windows;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class ResultViewModel : ViewModelBase
{
    private const double BarInnerWidth = 583;
    private static readonly CultureInfo Ua = new("uk-UA");

    private readonly IPCBuildService _buildService;
    private int? _existingBuildId;
    private IReadOnlyList<ResultItemViewModel> _items = Array.Empty<ResultItemViewModel>();
    private IReadOnlyList<BudgetSegmentViewModel> _budgetSegments = Array.Empty<BudgetSegmentViewModel>();
    private decimal _totalPrice;
    private PCBuildDto _currentBuild;

    public ResultViewModel(
        PCBuildDto build,
        decimal budget,
        IPCBuildService buildService,
        Action onBack,
        Action<ComponentCategory, ComponentDto?> onReplace,
        int? existingBuildId = null)
    {
        _buildService = buildService;
        _currentBuild = build;
        Budget = budget;
        _existingBuildId = existingBuildId;

        UpdateBuildData(build);

        BackCommand = new RelayCommand(_ => onBack());
        ReplaceCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is ComponentCategory category)
                {
                    var currentComp = _currentBuild.Components.FirstOrDefault(c => c.Category == category);
                    onReplace(category, currentComp);
                }
            });

        SaveBuildCommand = new AsyncRelayCommand(SaveBuildAsync);
    }

    public PCBuildDto CurrentBuild => _currentBuild;

    public IReadOnlyList<ResultItemViewModel> Items
    {
        get => _items;
        private set => SetProperty(ref _items, value);
    }

    public IReadOnlyList<BudgetSegmentViewModel> BudgetSegments
    {
        get => _budgetSegments;
        private set => SetProperty(ref _budgetSegments, value);
    }

    public decimal TotalPrice
    {
        get => _totalPrice;
        private set => SetProperty(ref _totalPrice, value);
    }

    public decimal Budget { get; }

    public string BudgetCaption =>
        $"{TotalPrice.ToString("N0", Ua)} / {Budget.ToString("N0", Ua)} UAH";

    public RelayCommand BackCommand { get; }
    public RelayCommand ReplaceCommand { get; }
    public AsyncRelayCommand SaveBuildCommand { get; }

    public void UpdateBuild(PCBuildDto newBuild)
    {
        _currentBuild = newBuild;
        UpdateBuildData(newBuild);
    }

    private void UpdateBuildData(PCBuildDto build)
    {
        TotalPrice = build.TotalPrice;

        Items = build.Components
            .OrderBy(c => c.Category)
            .Select(c => new ResultItemViewModel(c.Category, c.Name, c.Price))
            .ToList();

        BudgetSegments = Items
            .Select(i => new BudgetSegmentViewModel(
                i.Color,
                Budget > 0 ? (double)(i.Price / Budget) * BarInnerWidth : 0))
            .ToList();

        OnPropertyChanged(nameof(BudgetCaption));
    }

    private async Task SaveBuildAsync()
    {
        try
        {
            var name = $"Конфігурація CoreMeld ({TotalPrice.ToString("N0", Ua)} UAH)";
            var id = await _buildService.SaveOrUpdateBuildAsync(CurrentBuild, name, _existingBuildId);
            _existingBuildId = id; // Now it is tracked as our saved build
            MessageBox.Show($"Збірку #{id} успішно збережено в базі даних!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не вдалося зберегти збірку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
