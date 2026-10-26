using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class PartSelectorViewModel : ViewModelBase
{
    public PartSelectorViewModel(Action onBack)
    {
        BackCommand = new RelayCommand(_ => onBack());
    }

    public RelayCommand BackCommand { get; }
}
