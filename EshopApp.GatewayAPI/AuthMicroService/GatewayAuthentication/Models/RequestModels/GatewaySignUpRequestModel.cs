using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewaySignUpRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
