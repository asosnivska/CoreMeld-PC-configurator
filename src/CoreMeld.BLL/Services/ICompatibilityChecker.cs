using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface ICompatibilityChecker
{
    bool IsSocketCompatible(Cpu cpu, Motherboard motherboard);
    bool IsRamCompatible(Ram ram, Motherboard motherboard);
    bool IsPowerConnectorCompatible(Gpu? gpu, PowerSupply powerSupply);
}