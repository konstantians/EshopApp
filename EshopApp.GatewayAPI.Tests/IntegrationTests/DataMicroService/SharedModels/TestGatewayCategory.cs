using EshopApp.GatewayAPI.DataMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayCategory
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayProduct> Products { get; set; } = new List<GatewayProduct>();
}
