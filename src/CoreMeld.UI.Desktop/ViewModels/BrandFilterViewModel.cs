namespace CoreMeld.UI.Desktop.ViewModels;

public class BrandFilterViewModel : ViewModelBase
{
    private bool _isSelected;

    public BrandFilterViewModel(string name, Action onChanged)
    {
        Name = name;
        OnChanged = onChanged;
    }

    public string Name { get; }

    public Action OnChanged { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                OnChanged?.Invoke();
            }
        }
    }
}

