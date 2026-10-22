namespace CoreMeld.Domain.Models;

public class Motherboard: Component
{
    public int SocketId { get; set; }
    public int FormFactorId { get; set; }
    public string Chipset { get; set; } = String.Empty;
    public RamMemoryType MemoryType { get; set; }
    public int MemorySlots { get; set; }
    public int MaxMemoryGb { get; set; }
    public bool HasWifi { get; init; }
    
}