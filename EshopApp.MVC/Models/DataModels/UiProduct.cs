namespace EshopApp.MVC.Models.DataModels;

public class UiProduct
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UiCategory> Categories { get; set; } = new List<UiCategory>();
    public List<UiVariant> Variants { get; set; } = new List<UiVariant>();
}
