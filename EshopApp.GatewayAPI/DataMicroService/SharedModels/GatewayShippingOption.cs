namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayShippingOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public List<GatewayOrder> Orders { get; set; } = new List<GatewayOrder>();
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
