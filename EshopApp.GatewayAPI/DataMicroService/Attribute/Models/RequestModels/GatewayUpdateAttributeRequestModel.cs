using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Attribute.Models.RequestModels;

public class GatewayUpdateAttributeRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? VariantIds { get; set; }
}
