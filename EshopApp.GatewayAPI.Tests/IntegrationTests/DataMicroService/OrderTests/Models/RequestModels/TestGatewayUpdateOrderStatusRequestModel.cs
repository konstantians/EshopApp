namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests.Models.RequestModels;

internal class TestGatewayUpdateOrderStatusRequestModel
{
    public string? NewOrderStatus { get; set; }
    public string? OrderId { get; set; }
}
