namespace EshopApp.DataLibrary.Models;

public class VariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? VariantId { get; set; }
    public Variant? Variant { get; set; }
}
