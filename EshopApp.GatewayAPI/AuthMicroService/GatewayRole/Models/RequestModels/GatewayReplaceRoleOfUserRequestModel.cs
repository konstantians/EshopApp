using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayRole.Models.RequestModels;

public class GatewayReplaceRoleOfUserRequestModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? CurrentRoleId { get; set; }
    [Required]
    public string? NewRoleId { get; set; }
}
