namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayCartItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public string? CartId { get; set; }
    public TestGatewayCart? Cart { get; set; }
    public string? VariantId { get; set; }
    public TestGatewayVariant? Variant { get; set; }
}
