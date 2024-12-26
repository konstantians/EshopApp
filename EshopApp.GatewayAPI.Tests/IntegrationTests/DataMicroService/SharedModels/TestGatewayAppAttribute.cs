using EshopApp.GatewayAPI.DataMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayAppAttribute
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayVariant> Variants { get; set; } = new List<GatewayVariant>();
}
