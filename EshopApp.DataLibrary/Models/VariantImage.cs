namespace EshopApp.DataLibrary.Models;

public class VariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? VariantId { get; set; }
    public Variant? Variant { get; set; }
    public string? ImageId { get; set; }
    public AppImage? Image { get; set; }
}
