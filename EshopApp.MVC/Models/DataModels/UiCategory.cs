namespace EshopApp.MVC.Models.DataModels;

public class UiCategory
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UiProduct> Products { get; set; } = new List<UiProduct>();
}
