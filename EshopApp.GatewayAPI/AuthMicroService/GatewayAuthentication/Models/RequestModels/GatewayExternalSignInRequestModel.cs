using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayExternalSignInRequestModel
{
    [Required]
    public string? IdentityProviderName { get; set; }
    [Required]
    [Url]
    public string? ReturnUrl { get; set; }
}
