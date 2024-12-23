using System.ComponentModel.DataAnnotations;
using EshopApp.GatewayAPI.AuthMicroService.Models;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayRole.Models.RequestModels;

public class GatewayUpdateClaimsOfRoleRequestModel
{
    [Required]
    public string? RoleId { get; set; }
    [Required]
    public List<GatewayClaim> NewClaims { get; set; } = new List<GatewayClaim>();
}
