namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayVariant
{
    public string? Id { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public int? UnitsInStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ProductId { get; set; }
    public GatewayProduct? Product { get; set; }
    public string? DiscountId { get; set; }
    public GatewayDiscount? Discount { get; set; }
    public List<GatewayAppAttribute> Attributes { get; set; } = new List<GatewayAppAttribute>();
    public List<GatewayVariantImage> VariantImages { get; set; } = new List<GatewayVariantImage>();
    public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
    public List<GatewayCartItem> CartItems { get; set; } = new List<GatewayCartItem>();
}
