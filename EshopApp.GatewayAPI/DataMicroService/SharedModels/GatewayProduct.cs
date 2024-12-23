namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayProduct
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayCategory> Categories { get; set; } = new List<GatewayCategory>();
    public List<GatewayVariant> Variants { get; set; } = new List<GatewayVariant>();
}
