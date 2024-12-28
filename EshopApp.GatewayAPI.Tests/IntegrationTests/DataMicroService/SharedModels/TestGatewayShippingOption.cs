namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayShippingOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    //public List<Order> Orders { get; set; } = new List<Order>(); TODO will be added later
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
