using EshopApp.GatewayAPI.DataMicroService.Variant.Models.RequestModels;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Product.Models.RequestModels;

public class GatewayCreateProductRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Code { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string> CategoryIds { get; set; } = new List<string>();
    [Required]
    public GatewayCreateVariantRequestModel? CreateVariantRequestModel { get; set; }
}
