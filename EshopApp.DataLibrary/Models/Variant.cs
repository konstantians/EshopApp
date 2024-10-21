namespace EshopApp.DataLibrary.Models;

public class Variant
{
    public string? Id { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public int UnitsInStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? DiscountId { get; set; }
    public Discount? Discount { get; set; }
    public List<AppAttribute> Attributes { get; set; } = new List<AppAttribute>();
    public List<VariantImage> Images { get; set; } = new List<VariantImage>();
}
