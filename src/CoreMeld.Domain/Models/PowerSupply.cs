namespace CoreMeld.Domain.Models;

public class PowerSupply: Component
{
    public int FormFactorId { get; set; }
    public int Wattage { get; set; }
    public string EfficiencyRating { get; set; } = String.Empty;
}