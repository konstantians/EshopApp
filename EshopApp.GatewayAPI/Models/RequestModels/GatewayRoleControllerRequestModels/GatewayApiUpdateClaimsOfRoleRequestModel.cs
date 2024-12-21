using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayRoleControllerRequestModels;

public class GatewayApiUpdateClaimsOfRoleRequestModel
{
    [Required]
    public string? RoleId { get; set; }
    [Required]
    public List<GatewayClaim> NewClaims { get; set; } = new List<GatewayClaim>();
}
