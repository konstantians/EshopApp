using System.ComponentModel.DataAnnotations;
using EshopApp.GatewayAPI.DataMicroService.Order.Models.RequestModels;

namespace EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.RequestModels;

public class GatewayCreateCheckoutSessionRequestModel
{
    [Required]
    public string? SuccessUrl { get; set; }
    [Required]
    public string? CancelUrl { get; set; }
    [Required]
    public GatewayCreateOrderRequestModel? GatewayCreateOrderRequestModel { get; set; }
}
