using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.PaymentOption.Models.RequestModels;

public class GatewayCreatePaymentOptionRequestModel
{
    [MaxLength(50)]
    [Required]
    public string? Name { get; set; }
    [MaxLength(50)]
    [Required]
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The extra cost property must be a non-negative integer.")]
    public decimal? ExtraCost { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
}
