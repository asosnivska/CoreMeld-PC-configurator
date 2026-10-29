using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class ConfiguratorEngine : IConfiguratorEngine
{
    private readonly IPCBuildService _buildService;
    private readonly IComponentService _componentService;

    public ConfiguratorEngine(IPCBuildService buildService, IComponentService componentService)
    {
        _buildService = buildService;
        _componentService = componentService;
    }

    public PCBuildDto GenerateBuild(BuildRequestDto request)
    {
        return _buildService.GetDefaultBuildAsync().GetAwaiter().GetResult();
    }

    public PCBuildDto ReplaceComponent(PCBuild currentBuild, ComponentCategory category, Component newComponent)
    {
        throw new NotImplementedException("Use ReplaceComponent(PCBuildDto, ComponentDto) overload for presentation layer.");
    }

    public PCBuildDto ReplaceComponent(PCBuildDto currentBuild, ComponentDto newComponent)
    {
        return _buildService.ReplaceComponent(currentBuild, newComponent);
    }

    public Task<IReadOnlyList<ComponentDto>> GetComponentsByCategoryAsync(
        ComponentCategory category,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? sortByPriceAsc = null)
    {
        return _componentService.GetComponentsByCategoryAsync(category, search, minPrice, maxPrice, sortByPriceAsc);
    }

    public IReadOnlyList<PCBuildDto> GetPresetBuilds()
    {
        return _buildService.GetPresetBuildsAsync().GetAwaiter().GetResult();
    }

    public PCBuildDto GetDefaultBuild()
    {
        return _buildService.GetDefaultBuildAsync().GetAwaiter().GetResult();
    }
}
