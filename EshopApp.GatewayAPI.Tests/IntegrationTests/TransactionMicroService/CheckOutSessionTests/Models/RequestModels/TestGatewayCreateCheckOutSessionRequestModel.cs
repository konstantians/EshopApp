using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests.Models.RequestModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.CheckOutSessionTests.Models.RequestModels;

internal class TestGatewayCreateCheckOutSessionRequestModel
{
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public TestGatewayCreateOrderRequestModel? GatewayCreateOrderRequestModel { get; set; }
}
