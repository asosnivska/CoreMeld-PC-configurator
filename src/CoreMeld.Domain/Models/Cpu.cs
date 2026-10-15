namespace CoreMeld.Domain.Models;

public class Cpu: Component
{
    public int SocketId { get; set; }
    public int Cores { get; set; }
    public int Threads { get; set; }
    public decimal BaseClock { get; set; }
    public decimal BoostClock { get; set; }
    public int TdpWatts { get; set; }
    public bool HasIntegratedGpu { get; set; }
}