using System.Collections.ObjectModel;
using System.Windows;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class PartSelectorViewModel : ViewModelBase
{
    private readonly ComponentCategory _category;
    private readonly IComponentService _componentService;
    private readonly Action<ComponentDto> _onSelect;

    private PartSelectorItemViewModel? _currentItem;
    private string _searchText = string.Empty;
    private string _minPriceText = string.Empty;
    private string _maxPriceText = string.Empty;
    private int _selectedSortIndex = 0;
    private bool _isLoading;
    private bool _isEmpty;
    private string? _errorMessage;
    private CancellationTokenSource? _searchCts;

    public PartSelectorViewModel(
        ComponentCategory category,
        ComponentDto? currentComponent,
        IComponentService componentService,
        Action<ComponentDto> onSelect,
        Action onBack)
    {
        _category = category;
        _componentService = componentService;
        _onSelect = onSelect;

        CategoryTitle = GetCategoryTitle(category);

        if (currentComponent != null)
        {
            CurrentItem = new PartSelectorItemViewModel(currentComponent, isCurrent: true)
            {
                IsExpanded = true
            };
        }

        BackCommand = new RelayCommand(_ => onBack());

        SelectCommand = new RelayCommand(param =>
        {
            if (param is PartSelectorItemViewModel selectedItem)
            {
                var previousCurrent = CurrentItem;
                
                // Set new current item
                selectedItem.IsCurrent = true;
                selectedItem.IsExpanded = true;
                CurrentItem = selectedItem;

                // Move previous current back into alternatives list
                if (previousCurrent != null)
                {
                    previousCurrent.IsCurrent = false;
                    previousCurrent.IsExpanded = false;
                    Items.Remove(selectedItem);
                    if (!Items.Any(i => i.Component.Id == previousCurrent.Component.Id))
                    {
                        Items.Insert(0, previousCurrent);
                    }
                }
                else
                {
                    Items.Remove(selectedItem);
                }

                _onSelect(selectedItem.Component);
            }
        });

        ApplyFiltersCommand = new AsyncRelayCommand(LoadComponentsAsync);
        ResetFiltersCommand = new AsyncRelayCommand(ResetFiltersAsync);
        RetryCommand = new AsyncRelayCommand(InitializeAsync);

        _ = InitializeAsync();
    }

    public string CategoryTitle { get; }

    public PartSelectorItemViewModel? CurrentItem
    {
        get => _currentItem;
        set => SetProperty(ref _currentItem, value);
    }

    public ObservableCollection<PartSelectorItemViewModel> Items { get; } = new();
    public ObservableCollection<BrandFilterViewModel> BrandFilters { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                TriggerDebouncedSearch();
            }
        }
    }

    public string MinPriceText
    {
        get => _minPriceText;
        set => SetProperty(ref _minPriceText, value);
    }

    public string MaxPriceText
    {
        get => _maxPriceText;
        set => SetProperty(ref _maxPriceText, value);
    }

    public int SelectedSortIndex
    {
        get => _selectedSortIndex;
        set => SetProperty(ref _selectedSortIndex, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand BackCommand { get; }
    public RelayCommand SelectCommand { get; }
    public AsyncRelayCommand ApplyFiltersCommand { get; }
    public AsyncRelayCommand ResetFiltersCommand { get; }
    public AsyncRelayCommand RetryCommand { get; }

    public async Task InitializeAsync()
    {
        ErrorMessage = null;
        try
        {
            var brands = await _componentService.GetBrandsByCategoryAsync(_category);
            BrandFilters.Clear();
            foreach (var b in brands)
            {
                BrandFilters.Add(new BrandFilterViewModel(b, () => _ = LoadComponentsAsync()));
            }

            await LoadComponentsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Помилка завантаження: {ex.Message}";
        }
    }

    public async Task LoadComponentsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            decimal? minPrice = decimal.TryParse(MinPriceText, out var min) ? min : null;
            decimal? maxPrice = decimal.TryParse(MaxPriceText, out var max) ? max : null;
            bool? sortByPriceAsc = SelectedSortIndex switch
            {
                0 => true,
                1 => false,
                _ => null
            };

            var selectedBrands = BrandFilters.Where(b => b.IsSelected).Select(b => b.Name).ToList();
            IEnumerable<string>? brandsFilter = selectedBrands.Count > 0 ? selectedBrands : null;

            var components = await _componentService.GetComponentsByCategoryAsync(
                _category,
                SearchText,
                minPrice,
                maxPrice,
                sortByPriceAsc,
                brandsFilter);

            void UpdateItems()
            {
                Items.Clear();
                int currentId = CurrentItem?.Component.Id ?? -1;

                foreach (var comp in components)
                {
                    // If this item is the currently selected item pinned at the top, don't duplicate it in the lower list
                    if (comp.Id == currentId)
                        continue;

                    var vm = new PartSelectorItemViewModel(comp, isCurrent: false);
                    Items.Add(vm);
                }

                IsEmpty = Items.Count == 0 && CurrentItem == null;
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(UpdateItems);
            }
            else
            {
                UpdateItems();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Помилка пошуку: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ResetFiltersAsync()
    {
        SearchText = string.Empty;
        MinPriceText = string.Empty;
        MaxPriceText = string.Empty;
        SelectedSortIndex = 0;
        foreach (var b in BrandFilters)
        {
            b.IsSelected = false;
        }
        await LoadComponentsAsync();
    }

    private void TriggerDebouncedSearch()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(300, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                _ = LoadComponentsAsync();
            }
        }, TaskScheduler.Default);
    }

    public static string GetCategoryTitle(ComponentCategory category) => category switch
    {
        ComponentCategory.Cpu => "Процесор",
        ComponentCategory.Motherboard => "Материнська плата",
        ComponentCategory.Gpu => "Відеокарта",
        ComponentCategory.Ram => "Оперативна пам'ять",
        ComponentCategory.Storage => "Пристрій зберігання даних",
        ComponentCategory.CpuCooler => "Охолодження процесору",
        ComponentCategory.Case => "Корпус",
        ComponentCategory.PowerSupply => "Блок живлення",
        _ => "Деталі"
    };
}
