namespace CoreMeld.Domain.Models;

public class PCBuild
{
    public Cpu Cpu { get; set; }
    public Motherboard Motherboard { get; set; }
    public Gpu Gpu { get; set; }
    public Ram Ram { get; set; }
    public Storage Storage { get; set; }
    public CpuCooler CpuCooler { get; set; }
    public PCCase Case { get; set; }
    public PowerSupply PowerSupply { get; set; }
}