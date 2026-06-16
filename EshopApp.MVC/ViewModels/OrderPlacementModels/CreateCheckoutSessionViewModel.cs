using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class CreateCheckoutSessionViewModel
{
    [Required]
    public string? SuccessUrl { get; set; }
    [Required]
    public string? CancelUrl { get; set; }
    [Required]
    public CreateOrderViewModel? GatewayCreateOrderRequestModel { get; set; }
}
