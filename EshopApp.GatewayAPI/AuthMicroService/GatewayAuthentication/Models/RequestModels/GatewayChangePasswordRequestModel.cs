using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayChangePasswordRequestModel
{
    [Required]
    public string? CurrentPassword { get; set; }
    [Required]
    public string? NewPassword { get; set; }
}
