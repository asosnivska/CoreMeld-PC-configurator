using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;

namespace CoreMeld.UI.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private const bool PreviewResultScreen = false;
    private readonly StartViewModel _startViewModel;
    private ViewModelBase _currentViewModel;

    public MainViewModel(IConfiguratorEngine engine)
    {
        _startViewModel = new StartViewModel(engine, ShowResult);
        _currentViewModel = _startViewModel;

        if (PreviewResultScreen)
            ShowResult(SampleBuild(), 35_000m);
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }
    private void ShowResult(PCBuildDto build, decimal budget)
    {
        CurrentViewModel = new ResultViewModel(build, budget, ShowStart, ShowPartSelector);
    }

    private void ShowStart() => CurrentViewModel = _startViewModel;

    private void ShowPartSelector(ComponentCategory category)
    {
        CurrentViewModel = new PartSelectorViewModel(() => CurrentViewModel = _startViewModel);
    }

    private static PCBuildDto SampleBuild() => new(
        TotalPrice: 34_962m,
        TotalTdp: 250,
        IsCompatible: true,
        Components:
        [
            new(1, ComponentCategory.Cpu, "Процесор AMD Ryzen 5 5600X", "AMD", 6_784m, null, []),
            new(2, ComponentCategory.Motherboard, "Материнська плата Asus TUF Gaming A520M-Plus II", "Asus", 3_199m, null, []),
            new(3, ComponentCategory.Ram, "Модуль пам'яті G.Skill DDR4 16GB 3600MHz", "G.Skill", 6_748m, null, []),
            new(4, ComponentCategory.Gpu, "Відеокарта MSI GeForce GT1030 4GB", "MSI", 5_305m, null, []),
            new(5, ComponentCategory.Storage, "SSD 256GB Prologix S360", "Prologix", 1_809m, null, []),
            new(6, ComponentCategory.CpuCooler, "Кулер ID-Cooling SE-214-XT Basic", "ID-Cooling", 849m, null, []),
            new(7, ComponentCategory.Case, "Корпус Gamemax Destroyer MB", "Gamemax", 1_755m, null, []),
            new(8, ComponentCategory.PowerSupply, "Блок живлення 850W Chieftec PPG-850-C", "Chieftec", 4_769m, null, []),
        ]);
}
