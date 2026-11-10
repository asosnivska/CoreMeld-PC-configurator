using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface IConfiguratorEngine
{
    Task<PCBuildDto> GenerateBuildAsync(BuildRequestDto request);
    PCBuildDto ReplaceComponent(
        PCBuild currentBuild,
        ComponentCategory category,
        Component newComponent);
    
    IReadOnlyList<Component> FilterCompatibleComponents(IEnumerable<Component> pool, PCBuild currentBuild);
    Task<IReadOnlyList<Component>> FilterCompatibleComponentsAsync(IEnumerable<Component> pool, PCBuildDto currentBuild);
    Task<PCBuildDto> ReplaceComponentAsync(PCBuildDto currentBuild, ComponentCategory category, Component newComponent);
}