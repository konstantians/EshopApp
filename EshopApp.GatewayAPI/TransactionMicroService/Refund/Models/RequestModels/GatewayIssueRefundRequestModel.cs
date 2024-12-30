using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.TransactionMicroService.Refund.Models.RequestModels;

public class GatewayIssueRefundRequestModel
{
    [Required]
    public string? OrderId { get; set; }
    [Required]
    public string? PaymentIntentId { get; set; }
}
