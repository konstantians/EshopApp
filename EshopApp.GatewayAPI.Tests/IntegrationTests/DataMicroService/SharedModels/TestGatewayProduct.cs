namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayProduct
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestGatewayCategory> Categories { get; set; } = new List<TestGatewayCategory>();
    public List<TestGatewayVariant> Variants { get; set; } = new List<TestGatewayVariant>();
}
