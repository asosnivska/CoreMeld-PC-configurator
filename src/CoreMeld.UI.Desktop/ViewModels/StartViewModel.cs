using System.Globalization;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class StartViewModel : ViewModelBase
{
    private readonly IConfiguratorEngine _engine;
    private readonly Action<PCBuildDto, decimal> _showResult;

    private string _budgetText = string.Empty;
    private OptionItem<Category?> _selectedCategory;
    private OptionItem<StorageType?> _selectedStorageType;
    private OptionItem<int?> _selectedStorageCapacity;
    private OptionItem<CoolingType?> _selectedCooling;
    private string? _errorMessage;
    private string? _budgetError;
    private bool _isBusy;

    public StartViewModel(IConfiguratorEngine engine, Action<PCBuildDto, decimal> showResult)
    {
        _engine = engine;
        _showResult = showResult;

        CategoryOptions =
        [
            new(null, "Не обрано"),
            new(Category.HomeFullHd, "Домашній ПК (Full HD)"),
            new(Category.Home2K, "Домашній ПК (2K)"),
            new(Category.Office, "Офісні ПК"),
            new(Category.GamingFullHd, "Ігрові ПК Full HD (1080p)"),
            new(Category.Gaming2K, "Ігрові ПК Quad HD / 2K (1440p)"),
            new(Category.Gaming4K, "Ігрові ПК 4K (2160p)"),
            new(Category.Design3D, "3D-моделювання та монтаж 4K-відео"),
            new(Category.Rendering, "Складний рендеринг, VFX, Data Science та симуляції"),
            new(Category.Design2D, "2D-графіка та UI/UX дизайн"),
        ];

        StorageTypeOptions =
        [
            new(null, "Не обрано"),
            new(StorageType.SSD, "SSD (швидкий накопичувач)"),
            new(StorageType.HDD, "HDD (жорсткий диск)"),
        ];

        StorageCapacityOptions =
        [
            new(null, "Не обрано"),
            new(256, "256 ГБ"),
            new(500, "500 ГБ"),
            new(1000, "1 ТБ"),
            new(2000, "2 ТБ"),
        ];

        CoolingOptions =
        [
            new(null, "Не обрано"),
            new(CoolingType.Air, "Повітряне (кулер)"),
            new(CoolingType.Liquid, "Рідинне (СРО)"),
        ];

        _selectedCategory = CategoryOptions[0];
        _selectedStorageType = StorageTypeOptions[0];
        _selectedStorageCapacity = StorageCapacityOptions[0];
        _selectedCooling = CoolingOptions[0];

        BuildCommand = new AsyncRelayCommand(BuildAsync);
    }

    public IReadOnlyList<OptionItem<Category?>> CategoryOptions { get; }
    public IReadOnlyList<OptionItem<StorageType?>> StorageTypeOptions { get; }
    public IReadOnlyList<OptionItem<int?>> StorageCapacityOptions { get; }
    public IReadOnlyList<OptionItem<CoolingType?>> CoolingOptions { get; }

    public string BudgetText
    {
        get => _budgetText;
        set
        {
            if (SetProperty(ref _budgetText, value))
            {
                ErrorMessage = null;
                BudgetError = string.IsNullOrWhiteSpace(value) || TryParseBudget(value, out _)
                    ? null
                    : "Введіть додатне число";
            }
        }
    }

    public string? BudgetError
    {
        get => _budgetError;
        private set
        {
            if (SetProperty(ref _budgetError, value))
                OnPropertyChanged(nameof(HasBudgetError));
        }
    }

    public bool HasBudgetError => !string.IsNullOrEmpty(BudgetError);
    public OptionItem<Category?> SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public OptionItem<StorageType?> SelectedStorageType
    {
        get => _selectedStorageType;
        set => SetProperty(ref _selectedStorageType, value);
    }

    public OptionItem<int?> SelectedStorageCapacity
    {
        get => _selectedStorageCapacity;
        set => SetProperty(ref _selectedStorageCapacity, value);
    }

    public OptionItem<CoolingType?> SelectedCooling
    {
        get => _selectedCooling;
        set => SetProperty(ref _selectedCooling, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public AsyncRelayCommand BuildCommand { get; }

    private async Task BuildAsync()
    {
        ErrorMessage = null;

        if (!TryParseBudget(BudgetText, out var budget))
        {
            BudgetError = string.IsNullOrWhiteSpace(BudgetText)
                ? "Вкажіть бюджет"
                : "Введіть додатне число";

            ErrorMessage = BudgetError;
            return;
        }

        var request = new BuildRequestDto(
            Budget: budget,
            PreferredColor: null,
            RequireWifi: null,
            StorageType: SelectedStorageType.Value,
            CoolingType: SelectedCooling.Value,
            Category: SelectedCategory.Value,
            Priorities: null,
            RequestedRamGb: null,
            RequestedStorageGb: SelectedStorageCapacity.Value);

        IsBusy = true;
        try
        {
            var build = await Task.Run(() => _engine.GenerateBuild(request));
            _showResult(build, budget);
        }
        catch (NotImplementedException)
        {
            ErrorMessage = "Підбір збірки ще в розробці";
        }
        catch (Exception ex)
        {
            // TODO(#32): коли BudgetCalculator повертатиме мінімальну суму, показати її тут
            ErrorMessage = $"Не вдалося підібрати збірку: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool TryParseBudget(string input, out decimal budget)
    {
        var text = input.Replace(" ", "").Replace(" ", "").Replace(',', '.');

        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out budget)
               && budget is > 0 and <= 10_000_000m;
    }
}

