namespace EshopApp.MVC.Models.DataModels;

public class UiCart
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public List<UiCartItem> CartItems { get; set; } = new List<UiCartItem>();
}
