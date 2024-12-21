using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayRoleControllerRequestModels;

public class GatewayApiAddRoleToUserRequestModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? RoleId { get; set; }
}
