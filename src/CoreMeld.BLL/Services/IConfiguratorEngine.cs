using System.ComponentModel;
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
}