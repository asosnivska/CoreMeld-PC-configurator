using System.ComponentModel;
using CoreMeld.BLL.DTO;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class ConfiguratorEngine:IConfiguratorEngine
{
    public PCBuildDto GenerateBuild(BuildRequestDto request)
    {
        throw new NotImplementedException();
    }

    public PCBuildDto ReplaceComponent(PCBuild currentBuild, ComponentCategory category, Component newComponent)
    {
        throw new NotImplementedException();
    }
}