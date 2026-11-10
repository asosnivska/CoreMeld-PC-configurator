using CoreMeld.BLL.Settings;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class SpaceControlService: ISpaceControlService
{
    private readonly IFormFactorSpecsProvider _provider;

    public SpaceControlService(IFormFactorSpecsProvider provider)
    {
        _provider = provider;
    }
    
    public bool FitsInCase(PCCase pcCase, Motherboard motherboard, Gpu? gpu, CpuCooler cooler)
    {
        ArgumentNullException.ThrowIfNull(pcCase);
        ArgumentNullException.ThrowIfNull(motherboard);
        ArgumentNullException.ThrowIfNull(cooler);
        

        var fitsBoard = FitsBoard(pcCase, motherboard);
        var fitsGpu = gpu is null ? true : FitsGpu(pcCase, gpu);
        var fitsCooler = FitsCooler(pcCase, cooler);

        return fitsBoard && fitsGpu && fitsCooler;
    }

    public bool FitsBoard(PCCase pcCase, Motherboard motherboard)
    {
        
        ArgumentNullException.ThrowIfNull(pcCase);
        ArgumentNullException.ThrowIfNull(motherboard);

        if (pcCase.MaxBoardFormFactorId is null)
        {
            throw new InvalidOperationException("Форм-фактор корпусу не заповнено.");
        }

        var boardSize = _provider.GetBoardSize(motherboard.FormFactorId);
        var caseSize = _provider.GetBoardSize(pcCase.MaxBoardFormFactorId.Value);

        if (boardSize.LengthMm > caseSize.LengthMm || boardSize.WidthMm > caseSize.WidthMm)
        {
            return false;
        }

        return true;
    }

    public bool FitsGpu(PCCase pcCase, Gpu gpu)
    {
        ArgumentNullException.ThrowIfNull(pcCase);
        ArgumentNullException.ThrowIfNull(gpu);
        
        if (gpu.LengthMm > pcCase.MaxGpuLengthMm)
        {
            return false;
        }

        return true;
    }

    public bool FitsCooler(PCCase pcCase, CpuCooler cpuCooler)
    {
        
        ArgumentNullException.ThrowIfNull(pcCase);
        ArgumentNullException.ThrowIfNull(cpuCooler);
        if (cpuCooler.HeightMm > pcCase.MaxCpuCoolerHeightMm)
        {
            return false;
        }

        return true;
    }
}