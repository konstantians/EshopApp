using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.RequestModels;

public class GatewayCreateCheckoutSessionRequestModel
{
    [MaxLength(50)]
    public string? PaymentMethodType { get; set; }
    [Required]
    public string? SuccessUrl { get; set; }
    [Required]
    public string? CancelUrl { get; set; }
    public string? UserCouponCode { get; set; } //optional
    [Required]
    public GatewayCreateOrderRequestModel? GatewayCreateOrderRequestModel { get; set; }
}
