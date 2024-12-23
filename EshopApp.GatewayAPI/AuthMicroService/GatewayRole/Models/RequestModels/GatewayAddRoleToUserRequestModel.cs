using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayRole.Models.RequestModels;

public class GatewayAddRoleToUserRequestModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? RoleId { get; set; }
}
