namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.RefundTests.Models.RequestModels;
internal class TestGatewayHandleIssueRefundRequestModel
{
    public string? NewOrderStatus { get; set; }
    public string? PaymentIntentId { get; set; }
    public bool ShouldSendEmail { get; set; }
}
