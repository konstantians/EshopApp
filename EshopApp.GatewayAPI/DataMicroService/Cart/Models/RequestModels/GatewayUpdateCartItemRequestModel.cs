using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Cart.Models.RequestModels;

public class GatewayUpdateCartItemRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? CartItemId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "The quantity property have a value of 1 or greater.")]
    public int? Quantity { get; set; }
}
