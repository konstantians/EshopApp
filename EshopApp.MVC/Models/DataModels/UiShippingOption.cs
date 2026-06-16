namespace EshopApp.MVC.Models.DataModels;

public class UiShippingOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
}
