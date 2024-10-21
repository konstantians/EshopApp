namespace EshopApp.DataLibrary.Models;

public class Category
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime ModifiedAt { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}
