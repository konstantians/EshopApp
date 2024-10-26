namespace EshopApp.DataLibrary.Models;
public class AppImage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool ShouldNotShowInGallery { get; set; }
    public bool ExistsInOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<VariantImage> VariantImages { get; set; } = new List<VariantImage>();
}
