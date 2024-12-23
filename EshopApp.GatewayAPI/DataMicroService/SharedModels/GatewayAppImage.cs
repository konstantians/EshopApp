namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayAppImage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayVariantImage> VariantImages { get; set; } = new List<GatewayVariantImage>();
    //public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); will be added eventually
}
