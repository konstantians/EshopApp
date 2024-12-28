namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayCartItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public string? CartId { get; set; }
    public GatewayCart? Cart { get; set; }
    public string? VariantId { get; set; }
    public GatewayVariant? Variant { get; set; }
}
