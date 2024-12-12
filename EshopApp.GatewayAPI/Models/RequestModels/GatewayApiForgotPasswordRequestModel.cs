using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels;

public class GatewayApiForgotPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
