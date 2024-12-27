namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayVariant
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
    public TestGatewayProduct? Product { get; set; }
    public string? DiscountId { get; set; }
    public TestGatewayDiscount? Discount { get; set; }
    public List<TestGatewayAppAttribute> Attributes { get; set; } = new List<TestGatewayAppAttribute>();
    public List<TestGatewayVariantImage> VariantImages { get; set; } = new List<TestGatewayVariantImage>();
    //public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    //public List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
