using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface ISpaceControlService
{
    bool FitsInCase(PCCase pcCase, Motherboard motherboard, Gpu? gpu, CpuCooler cooler);
}