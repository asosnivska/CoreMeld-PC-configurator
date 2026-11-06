using CoreMeld.Domain.Enums;

namespace CoreMeld.Domain.Models;

public class Ram: Component
{
    public RamMemoryType RamMemoryType { get; set; }
    public int CapacityGb { get; set; }
    public int ModuleCount { get; set; }
    public int FrequencyMhz { get; set; }
}