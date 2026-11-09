using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface ISpaceControlService
{
    bool FitsInCase(PCCase pcCase, Motherboard motherboard, Gpu? gpu, CpuCooler cooler);
    bool FitsBoard(PCCase pcCase, Motherboard motherboard);
    bool FitsGpu(PCCase pcCase, Gpu gpu);
    bool FitsCooler(PCCase pcCase, CpuCooler cpuCooler);
}