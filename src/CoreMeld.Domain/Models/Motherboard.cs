namespace CoreMeld.Domain.Models;

public class Motherboard: Component
{
    public int SocketId { get; set; }
    public int FormFactorId { get; set; }
    public string Chipset { get; set; } = String.Empty;
    public string MemoryType { get; set; }= String.Empty;
    public int MemorySlots { get; set; }
    public int MaxMemoryGb { get; set; }
    public bool HasWifi { get; init; }
    
}