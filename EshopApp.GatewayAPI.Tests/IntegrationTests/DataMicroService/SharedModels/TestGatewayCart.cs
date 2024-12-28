namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayCart
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TestGatewayCartItem> CartItems { get; set; } = new List<TestGatewayCartItem>();
}
