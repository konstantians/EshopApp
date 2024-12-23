using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Category.Models.RequestModels;

public class GatewayUpdateCategoryRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? ProductIds { get; set; }
}
