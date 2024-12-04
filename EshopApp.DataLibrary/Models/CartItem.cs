namespace EshopApp.DataLibrary.Models;
public class CartItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public string? CartId { get; set; }
    public Cart? Cart { get; set; }
    public string? VariantId { get; set; }
    public Variant? Variant { get; set; }
}
