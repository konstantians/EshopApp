using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayCreateVariantImageRequestModel
{
    public bool IsThumbNail { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ImageId { get; set; }
}
