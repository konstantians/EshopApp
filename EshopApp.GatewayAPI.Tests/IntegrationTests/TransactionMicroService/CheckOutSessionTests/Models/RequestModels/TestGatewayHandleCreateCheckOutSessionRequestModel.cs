namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.CheckOutSessionTests.Models.RequestModels;

internal class TestGatewayHandleCreateCheckOutSessionRequestModel
{
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    public string? NewOrderStatus { get; set; }
    public string? NewPaymentStatus { get; set; }
    public string? PaymentCurrency { get; set; }
    public decimal? AmountPaidInEuro { get; set; }
    public decimal? NetAmountPaidInEuro { get; set; }
    public bool ShouldSendEmail { get; set; }
}
