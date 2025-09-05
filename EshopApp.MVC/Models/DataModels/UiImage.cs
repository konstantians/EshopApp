namespace EshopApp.MVC.Models.DataModels;

public class UiImage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UiVariantImage> VariantImages { get; set; } = new List<UiVariantImage>();
    //public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
}
