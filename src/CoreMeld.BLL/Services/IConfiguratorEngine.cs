using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface IConfiguratorEngine
{
    PCBuildDto GenerateBuild(BuildRequestDto request);

    PCBuildDto ReplaceComponent(
        PCBuild currentBuild,
        ComponentCategory category,
        Component newComponent);

    PCBuildDto ReplaceComponent(
        PCBuildDto currentBuild,
        ComponentDto newComponent);

    Task<IReadOnlyList<ComponentDto>> GetComponentsByCategoryAsync(
        ComponentCategory category,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? sortByPriceAsc = null);

    IReadOnlyList<PCBuildDto> GetPresetBuilds();

    PCBuildDto GetDefaultBuild();
}
