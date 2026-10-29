using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface IPCBuildService
{
    Task<PCBuildDto> GetDefaultBuildAsync();

    Task<IReadOnlyList<PCBuildDto>> GetPresetBuildsAsync();

    PCBuildDto ReplaceComponent(PCBuildDto currentBuild, ComponentDto newComponent);

    Task<int> SaveOrUpdateBuildAsync(PCBuildDto build, string name, int? existingBuildId);

    Task<IReadOnlyList<BuildSummary>> GetMySavedBuildsAsync();

    Task<IReadOnlyList<BuildSummary>> GetTopBuildsAsync();

    Task<PCBuildDto> LoadBuildByIdAsync(int buildId);
}
