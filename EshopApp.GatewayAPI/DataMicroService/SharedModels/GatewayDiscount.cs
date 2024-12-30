namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayDiscount
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayVariant> Variants { get; set; } = new List<GatewayVariant>();
    public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
}
