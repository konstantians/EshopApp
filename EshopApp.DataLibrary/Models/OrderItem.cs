namespace EshopApp.DataLibrary.Models;
public class OrderItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPriceAtOrder { get; set; }
    public int? DiscountPercentageAtOrder { get; set; }
    public string? OrderId { get; set; }
    public Order? Order { get; set; }
    public string? ImageId { get; set; }
    public AppImage? Image { get; set; }
    public string? VariantId { get; set; }
    public Variant? Variant { get; set; }
    public string? DiscountId { get; set; }
    public Discount? Discount { get; set; }
}
