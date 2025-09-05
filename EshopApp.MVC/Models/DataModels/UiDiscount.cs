namespace EshopApp.MVC.Models.DataModels;

public class UiDiscount
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<UiVariant> Variants { get; set; } = new List<UiVariant>();
    //public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
}
