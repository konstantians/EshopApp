using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.RequestModels;

public class GatewayHandleCreateCheckOutSessionRequestModel
{
    [Required]
    [MaxLength(100)]
    public string? PaymentProcessorSessionId { get; set; }
    [MaxLength(100)]
    public string? PaymentProcessorPaymentIntentId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? NewOrderStatus { get; set; }
    [MaxLength(50)]
    public string? NewPaymentStatus { get; set; }
    [MaxLength(5)]
    public string? PaymentCurrency { get; set; }
    public decimal AmountPaidInEuro { get; set; }
    public decimal NetAmountPaidInEuro { get; set; }
    public bool ShouldSendEmail { get; set; }
}
