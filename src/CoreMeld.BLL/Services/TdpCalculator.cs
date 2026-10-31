using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class TdpCalculator: ITdpCalculator
{

    private const int OtherComponentsTdpWatts = 100;
    private const decimal PsuReserveMultiplier = 1.20m;
    private const decimal PsuWattageStep = 50m;
        
    public int CalculateTotalTdp(Cpu cpu, Gpu? gpu)
    {
        if (cpu is null)
        {
            throw new ArgumentNullException(nameof(cpu), "Без процесора збірка неможлива.");
        }

        var gpuTdp = gpu?.TdpWatts ?? 0;

        return cpu.TdpWatts + gpuTdp + OtherComponentsTdpWatts;
    }

    public int GetRecommendedPsuWattage(int totalTdp)
    {
        if (totalTdp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTdp), totalTdp, "Сумарний TDP має бути більшим за 0.");
        }

        var needed = totalTdp * PsuReserveMultiplier;
        
        return (int)(Math.Ceiling(needed / PsuWattageStep) * PsuWattageStep);

    }
}