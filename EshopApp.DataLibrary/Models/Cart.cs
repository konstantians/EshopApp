namespace EshopApp.DataLibrary.Models;
public class Cart
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
