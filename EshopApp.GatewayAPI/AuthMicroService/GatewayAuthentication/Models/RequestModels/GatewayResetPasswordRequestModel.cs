using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayResetPasswordRequestModel
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? Token { get; set; }
    [Required]
    public string? Password { get; set; }
}
