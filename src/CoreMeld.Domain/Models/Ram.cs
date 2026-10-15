namespace CoreMeld.Domain.Models;

public class Ram: Component
{
    public string MemoryType { get; set; } = String.Empty;
    public int CapacityGb { get; set; }
    public int ModuleCount { get; set; }
    public int FrequencyMhz { get; set; }
}