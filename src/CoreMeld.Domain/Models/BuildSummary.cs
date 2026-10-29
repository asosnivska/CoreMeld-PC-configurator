namespace CoreMeld.Domain.Models;

public class BuildSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public int TotalTdpWatts { get; set; }
    public DateTime CreatedAt { get; set; }
}

