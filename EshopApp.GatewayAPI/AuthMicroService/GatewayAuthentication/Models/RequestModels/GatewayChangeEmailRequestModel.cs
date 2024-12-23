using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayChangeEmailRequestModel
{
    [Required]
    [EmailAddress]
    public string? NewEmail { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
