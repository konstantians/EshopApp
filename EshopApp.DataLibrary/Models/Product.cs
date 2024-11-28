namespace EshopApp.DataLibrary.Models;

public class Product
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Variant> Variants { get; set; } = new List<Variant>();
}
