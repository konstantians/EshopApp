namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayCart
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GatewayCartItem> CartItems { get; set; } = new List<GatewayCartItem>();
}
