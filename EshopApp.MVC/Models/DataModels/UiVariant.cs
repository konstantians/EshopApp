namespace EshopApp.MVC.Models.DataModels;

public class UiVariant
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
    public UiProduct? Product { get; set; }
    public string? DiscountId { get; set; }
    public UiDiscount? Discount { get; set; }
    public List<UiAttribute> Attributes { get; set; } = new List<UiAttribute>();
    public List<UiVariantImage> VariantImages { get; set; } = new List<UiVariantImage>();
    //public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
    //public List<GatewayCartItem> CartItems { get; set; } = new List<GatewayCartItem>();
}
