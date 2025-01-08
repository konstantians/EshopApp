namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayPaymentDetails
{
    public string? Id { get; set; }
    public string? PaymentStatus { get; set; } //paid, unpaid, pending
    public string? PaymentCurrency { get; set; } //usd, eur, gbp, aud, cad
    public decimal? AmountPaidInEuro { get; set; }
    public decimal? NetAmountPaidInEuro { get; set; } //this is amount paid in euro - stripe fees
    public decimal? PaymentOptionExtraCostAtOrder { get; set; }
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    public string? PaymentOptionId { get; set; }
    public TestGatewayPaymentOption? PaymentOption { get; set; }
    public string? OrderId { get; set; }
    public TestGatewayOrder? Order { get; set; }
}
