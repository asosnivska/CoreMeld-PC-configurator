using System.Collections.ObjectModel;
using System.Windows;
using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Services;
using CoreMeld.UI.Desktop.Commands;

namespace CoreMeld.UI.Desktop.ViewModels;

public class BuildsViewModel : ViewModelBase
{
    private readonly IPCBuildService _buildService;
    private readonly Action _onGoToConstructor;
    private readonly Action<PCBuildDto, decimal, int?> _onOpenBuild;

    private bool _isLoading;
    private bool _hasMyBuilds;
    private bool _hasTopBuilds;

    public BuildsViewModel(
        IPCBuildService buildService,
        Action onGoToConstructor,
        Action<PCBuildDto, decimal, int?> onOpenBuild)
    {
        _buildService = buildService;
        _onGoToConstructor = onGoToConstructor;
        _onOpenBuild = onOpenBuild;

        GoToConstructorCommand = new RelayCommand(_ => _onGoToConstructor());
        RefreshCommand = new AsyncRelayCommand(LoadBuildsAsync);

        _ = LoadBuildsAsync();
    }

    public ObservableCollection<BuildCardViewModel> MySavedBuilds { get; } = new();
    public ObservableCollection<BuildCardViewModel> TopBuilds { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasMyBuilds
    {
        get => _hasMyBuilds;
        set => SetProperty(ref _hasMyBuilds, value);
    }

    public bool HasTopBuilds
    {
        get => _hasTopBuilds;
        set => SetProperty(ref _hasTopBuilds, value);
    }

    public RelayCommand GoToConstructorCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }

    public async Task LoadBuildsAsync()
    {
        IsLoading = true;
        try
        {
            var myBuilds = await _buildService.GetMySavedBuildsAsync();
            var topBuilds = await _buildService.GetTopBuildsAsync();

            void Update()
            {
                MySavedBuilds.Clear();
                foreach (var b in myBuilds)
                {
                    // Opened from My Builds: existingBuildId is b.Id
                    MySavedBuilds.Add(new BuildCardViewModel(b, new AsyncRelayCommand(async () => await OpenBuildAsync(b.Id, b.TotalPrice, existingBuildId: b.Id))));
                }
                HasMyBuilds = MySavedBuilds.Count > 0;

                TopBuilds.Clear();
                foreach (var b in topBuilds)
                {
                    // Opened from Top Builds: existingBuildId is null (saving creates a new build)
                    TopBuilds.Add(new BuildCardViewModel(b, new AsyncRelayCommand(async () => await OpenBuildAsync(b.Id, b.TotalPrice, existingBuildId: null))));
                }
                HasTopBuilds = TopBuilds.Count > 0;
            }

            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(Update);
            }
            else
            {
                Update();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OpenBuildAsync(int buildId, decimal price, int? existingBuildId)
    {
        IsLoading = true;
        try
        {
            var build = await _buildService.LoadBuildByIdAsync(buildId);
            _onOpenBuild(build, price, existingBuildId);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
