namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayVariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? VariantId { get; set; }
    public GatewayVariant? Variant { get; set; }
    public string? ImageId { get; set; }
    public GatewayAppImage? Image { get; set; }
}
