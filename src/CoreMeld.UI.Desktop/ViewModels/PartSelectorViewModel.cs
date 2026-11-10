using System.Diagnostics;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class PartSelectorViewModel : ViewModelBase
{
    private readonly IConfiguratorEngine _engine;
    private readonly IComponentRepository _components;
    private readonly PCBuildDto _build;
    private readonly ComponentCategory _category;
    private readonly Action<PCBuildDto> _onSelected;

    private IReadOnlyList<PartItemViewModel> _items = [];
    private bool _isLoading = true;
    private string? _errorMessage;

    public PartSelectorViewModel(
        IConfiguratorEngine engine,
        IComponentRepository components,
        PCBuildDto build,
        ComponentCategory category,
        Action<PCBuildDto> onSelected,
        Action onBack)
    {
        _engine = engine;
        _components = components;
        _build = build;
        _category = category;
        _onSelected = onSelected;

        Title = new ResultItemViewModel(category, string.Empty, 0m).CategoryName;

        BackCommand = new RelayCommand(_ => onBack());
        SelectCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is PartItemViewModel item)
                    _ = SelectAsync(item);
            },
            parameter => parameter is PartItemViewModel { IsCurrent: false });

        _ = LoadAsync();
    }

    public string Title { get; }

    public IReadOnlyList<PartItemViewModel> Items
    {
        get => _items;
        private set => SetProperty(ref _items, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
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

    public RelayCommand BackCommand { get; }
    public RelayCommand SelectCommand { get; }

    private async Task LoadAsync()
    {
        try
        {
            var all = await _components.GetAllAsync();
            var pool = all.Where(c => c.CategoryId == (int)_category).ToList();

            var compatible = await _engine.FilterCompatibleComponentsAsync(pool, _build);

            var currentId = _build.Components.FirstOrDefault(c => c.Category == _category)?.Id;

            Items = compatible
                .OrderByDescending(c => c.Id == currentId)
                .ThenBy(c => c.Price)
                .Select(c => new PartItemViewModel(c, ComponentDtoMapper.ToDto(c), c.Id == currentId))
                .ToList();

            if (Items.Count == 0)
                ErrorMessage = "Немає сумісних деталей для цієї категорії";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ErrorMessage = "Не вдалося завантажити список деталей";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SelectAsync(PartItemViewModel item)
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var newBuild = await _engine.ReplaceComponentAsync(_build, _category, item.Component);
            _onSelected(newBuild);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ErrorMessage = "Не вдалося замінити деталь. Спробуйте іншу";
            IsLoading = false;
        }
    }
}