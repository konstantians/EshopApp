namespace EshopApp.DataLibrary.Models;

public class AppAttribute
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<Variant> Variants { get; set; } = new List<Variant>();
}
