namespace EshopApp.TransactionLibraryAPI.Models.ResponseModels;

public class HandleCheckOutSessionResponseModel
{
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    public string? NewOrderStatus { get; set; }
    public string? NewPaymentStatus { get; set; }
    public string? PaymentCurrency { get; set; }
    public decimal AmountPaidInEuro { get; set; }
    public decimal NetAmountPaidInEuro { get; set; }
}
