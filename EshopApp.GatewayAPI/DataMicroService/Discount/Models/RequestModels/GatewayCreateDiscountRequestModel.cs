using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Discount.Models.RequestModels;

public class GatewayCreateDiscountRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    [Required]
    [Range(1, 99, ErrorMessage = "Percentage must be between 1 and 99")]
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
}
