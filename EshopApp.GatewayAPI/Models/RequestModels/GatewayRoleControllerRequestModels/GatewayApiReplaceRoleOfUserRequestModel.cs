using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayRoleControllerRequestModels;

public class GatewayApiReplaceRoleOfUserRequestModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? CurrentRoleId { get; set; }
    [Required]
    public string? NewRoleId { get; set; }
}
