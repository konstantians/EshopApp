namespace EshopApp.MVC.Models.DataModels;

public class UiCartItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public UiVariant? Variant { get; set; }
}
