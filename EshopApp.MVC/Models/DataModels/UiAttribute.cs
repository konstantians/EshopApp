namespace EshopApp.MVC.Models.DataModels;

public class UiAttribute
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UiVariant> Variants { get; set; } = new List<UiVariant>();
}
