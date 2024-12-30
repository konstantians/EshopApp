namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayOrderItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPriceAtOrder { get; set; }
    public int? DiscountPercentageAtOrder { get; set; }
    public string? OrderId { get; set; }
    public GatewayOrder? Order { get; set; }
    public string? ImageId { get; set; }
    public GatewayAppImage? Image { get; set; }
    public string? VariantId { get; set; }
    public GatewayVariant? Variant { get; set; }
    public string? DiscountId { get; set; }
    public GatewayDiscount? Discount { get; set; }
}
