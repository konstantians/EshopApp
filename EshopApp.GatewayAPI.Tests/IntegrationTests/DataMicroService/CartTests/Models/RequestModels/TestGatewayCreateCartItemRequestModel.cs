namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CartTests.Models.RequestModels;
internal class TestGatewayCreateCartItemRequestModel
{
    public int Quantity { get; set; }
    public string? CartId { get; set; }
    public string? VariantId { get; set; }
}
