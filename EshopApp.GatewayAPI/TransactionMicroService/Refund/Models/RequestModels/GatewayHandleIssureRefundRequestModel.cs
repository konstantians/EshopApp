using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.TransactionMicroService.Refund.Models.RequestModels;

public class GatewayHandleIssureRefundRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? NewOrderStatus { get; set; }
    [Required]
    [MaxLength(50)]
    public string? PaymentIntentId { get; set; }
    public bool ShouldSendEmail { get; set; }
}
