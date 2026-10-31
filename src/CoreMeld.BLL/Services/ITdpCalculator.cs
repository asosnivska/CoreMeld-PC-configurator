using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public interface ITdpCalculator
{
    int CalculateTotalTdp(Cpu cpu, Gpu? gpu);
    int GetRecommendedPsuWattage(int totalTdp);
}