namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.RefundTests.Models.RequestModels;

internal class TestGatewayIssueRefundRequestModel
{
    public string? OrderId { get; set; }
    public string? PaymentIntentId { get; set; }
}
