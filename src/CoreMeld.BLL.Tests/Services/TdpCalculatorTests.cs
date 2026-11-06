using CoreMeld.BLL.Services;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Tests.Services;

public class TdpCalculatorTests
{

    private readonly ITdpCalculator _calculator = new TdpCalculator();
    
    [Fact]
    public void CalculateTotalTdp_CpuAndGpu_AddsOtherComponentsAllowance()
    {
        Assert.Equal(365, _calculator.CalculateTotalTdp(new Cpu { TdpWatts = 65 }, new Gpu {TdpWatts = 200}));
    }

    [Fact]
    public void CalculateTotalTdp_NoGpu_UsesOnlyCpuAndAllowance()
    {
        Assert.Equal(165, _calculator.CalculateTotalTdp(new Cpu { TdpWatts = 65 }, null));
    }

    [Fact]
    public void CalculateTotalTdp_NullCpu_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => { _calculator.CalculateTotalTdp(null!, null); });
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateTotalTdp_CpuNonPositiveTdp_Throw(int cpuTdp)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            _calculator.CalculateTotalTdp(new Cpu { TdpWatts = cpuTdp }, new Gpu { TdpWatts = 200 });
        });
        
    }
    
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateTotalTdp_GpuNonPositiveTdp_Throws(int gpuTdp)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            _calculator.CalculateTotalTdp(new Cpu { TdpWatts = 100 }, new Gpu { TdpWatts = gpuTdp });
        });
        
    }


    [Theory]
    [InlineData(250, 300)]
    [InlineData(251, 350)]
    [InlineData(400, 500)]
    [InlineData(1000, 1200)]
    [InlineData(1, 50)]
    public void GetRecommendedPsuWattage_ValidTdp_ReturnsReserveRoundedUpTo50(int totalTdp, int expected)
    {
        Assert.Equal(expected, _calculator.GetRecommendedPsuWattage(totalTdp));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetRecommendedPsuWattage_NonPositive_Throws(int totalTdp)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _calculator.GetRecommendedPsuWattage(totalTdp);
        });
    }

    [Theory]
    [InlineData(100)]
    [InlineData(365)]
    [InlineData(800)]
    public void GetRecommendedPsuWattage_AnyTdp_ResultIsMultipleOf50AndHasEnoughReserve(int totalTdp)
    {
        var result = _calculator.GetRecommendedPsuWattage(totalTdp);
        
        Assert.Equal(0, result % 50);
        Assert.True(result >= totalTdp * 1.15m);
    }
}