using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Attribute.Models.RequestModels;

public class GatewayCreateAttributeRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
}
