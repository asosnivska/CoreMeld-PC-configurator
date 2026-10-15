namespace CoreMeld.Domain.Models;

public class Storage: Component
{
    public string DriveType { get; set; } = String.Empty;
    public string InterfaceType { get; set; } = String.Empty;
    public int CapacityGb { get; set; }
}