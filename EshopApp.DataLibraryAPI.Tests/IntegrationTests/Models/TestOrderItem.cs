namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestOrderItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPriceAtOrder { get; set; }
    public int? DiscountPercentageAtOrder { get; set; }
    public string? OrderId { get; set; }
    public TestOrder? Order { get; set; }
    public string? ImageId { get; set; }
    public TestAppImage? Image { get; set; }
    public string? VariantId { get; set; }
    public TestVariant? Variant { get; set; }
    public string? DiscountId { get; set; }
    public TestDiscount? Discount { get; set; }
}
