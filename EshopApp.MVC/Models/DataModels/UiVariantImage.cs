namespace EshopApp.MVC.Models.DataModels;

public class UiVariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? VariantId { get; set; }
    public UiVariant? Variant { get; set; }
    public string? ImageId { get; set; }
    public UiImage? Image { get; set; }
}
