namespace CoreMeld.Domain.Models;

public abstract class Components
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}