namespace CoreMeld.Domain.Models;

public class Gpu: Component
{
    public int VramGb { get; set; }
    public int TdpWatts { get; set; }
    public int LengthMm { get; set; }
    public int RecommendedPsuWatts { get; set; }
    public string MemoryType { get; set; } = String.Empty;

}