using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayForgotPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
