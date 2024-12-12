using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels;

public class GatewayApiChangeEmailRequestModel
{
    [Required]
    [EmailAddress]
    public string? NewEmail { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
