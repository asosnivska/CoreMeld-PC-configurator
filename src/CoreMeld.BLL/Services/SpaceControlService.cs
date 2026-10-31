using System.Text.Json.Serialization;
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

        if (gpu is not null && gpu.LengthMm > pcCase.MaxGpuLengthMm)
        {
            return false;
        }

        if (cooler.HeightMm > pcCase.MaxCpuCoolerHeightMm)
        {
            return false;
        }

        return true;
    }
}