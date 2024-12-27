namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayDiscount
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestGatewayVariant> Variants { get; set; } = new List<TestGatewayVariant>();
    //public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); TODO add this later
}
