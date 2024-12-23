using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Category.Models.RequestModels;

public class GatewayCreateCategoryRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
}
