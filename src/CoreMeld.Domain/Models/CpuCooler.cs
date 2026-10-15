namespace CoreMeld.Domain.Models;

public class CpuCooler: Component
{
    public int HeightMm { get; set; }
    public int TdpRatingWatts { get; set; }
    public int? FanRpm { get; set; }
    public decimal? NoiseLevelDb { get; set; }
    public int SupportedSocketIds { get; set; }
    
}