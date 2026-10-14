namespace CoreMeld.Domain.Models;

public class Component
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
