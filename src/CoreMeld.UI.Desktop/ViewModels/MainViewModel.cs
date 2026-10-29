using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;

namespace CoreMeld.UI.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IPCBuildService _buildService;
    private readonly IComponentService _componentService;
    private readonly IConfiguratorEngine _engine;

    private ViewModelBase _currentViewModel;
    private ResultViewModel? _resultViewModel;

    public MainViewModel(
        IPCBuildService buildService,
        IComponentService componentService,
        IConfiguratorEngine engine)
    {
        _buildService = buildService;
        _componentService = componentService;
        _engine = engine;

        // Start with the Builds Catalog view
        _currentViewModel = new BuildsViewModel(
            _buildService,
            onGoToConstructor: ShowStart,
            onOpenBuild: (build, budget, existingBuildId) => ShowResult(build, budget, existingBuildId));
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public void ShowBuilds()
    {
        CurrentViewModel = new BuildsViewModel(
            _buildService,
            onGoToConstructor: ShowStart,
            onOpenBuild: (build, budget, existingBuildId) => ShowResult(build, budget, existingBuildId));
    }

    public void ShowStart()
    {
        CurrentViewModel = new StartViewModel(
            _engine,
            showResult: (build, budget) => ShowResult(build, budget, existingBuildId: null),
            onBack: ShowBuilds);
    }

    public void ShowResult(PCBuildDto build, decimal budget, int? existingBuildId = null)
    {
        _resultViewModel = new ResultViewModel(
            build,
            budget,
            _buildService,
            onBack: ShowBuilds,
            onReplace: ShowPartSelector,
            existingBuildId: existingBuildId);

        CurrentViewModel = _resultViewModel;
    }

    private void ShowPartSelector(ComponentCategory category, ComponentDto? currentComp)
    {
        CurrentViewModel = new PartSelectorViewModel(
            category,
            currentComp,
            _componentService,
            onSelect: selectedComponent =>
            {
                if (_resultViewModel != null)
                {
                    var updated = _buildService.ReplaceComponent(_resultViewModel.CurrentBuild, selectedComponent);
                    _resultViewModel.UpdateBuild(updated);
                    CurrentViewModel = _resultViewModel;
                }
            },
            onBack: () =>
            {
                if (_resultViewModel != null)
                    CurrentViewModel = _resultViewModel;
                else
                    ShowBuilds();
            });
    }
}
