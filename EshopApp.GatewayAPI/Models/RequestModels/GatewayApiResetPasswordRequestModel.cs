using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels;

public class GatewayApiResetPasswordRequestModel
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? Token { get; set; }
    [Required]
    public string? Password { get; set; }
}
