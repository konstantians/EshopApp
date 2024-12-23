using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Image.Models.RequestModels;

public class GatewayRestoreImagesRequestModel
{
    [Required]
    public List<string> ImageIds { get; set; } = new List<string>();
}
