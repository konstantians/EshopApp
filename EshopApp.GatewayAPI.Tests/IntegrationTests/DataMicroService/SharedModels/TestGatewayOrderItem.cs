namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayOrderItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPriceAtOrder { get; set; }
    public int? DiscountPercentageAtOrder { get; set; }
    public string? OrderId { get; set; }
    public TestGatewayOrder? Order { get; set; }
    public string? ImageId { get; set; }
    public TestGatewayAppImage? Image { get; set; }
    public string? VariantId { get; set; }
    public TestGatewayVariant? Variant { get; set; }
    public string? DiscountId { get; set; }
    public TestGatewayDiscount? Discount { get; set; }
}
