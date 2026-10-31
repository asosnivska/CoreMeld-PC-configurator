using CoreMeld.Domain.Enums;

namespace CoreMeld.Domain.Models;

public class Gpu: Component
{
    public int VramGb { get; set; }
    public int TdpWatts { get; set; }
    public int LengthMm { get; set; }
    public int RecommendedPsuWatts { get; set; }
    public VideoMemoryType MemoryType { get; set; }
    public PowerConnectorType? PowerConnectorType { get; set; }
    public int? PowerConnectorCount { get; set; }

}