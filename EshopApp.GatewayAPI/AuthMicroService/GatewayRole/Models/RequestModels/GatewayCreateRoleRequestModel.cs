using System.ComponentModel.DataAnnotations;
using EshopApp.GatewayAPI.AuthMicroService.Models;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayRole.Models.RequestModels;

public class GatewayCreateRoleRequestModel
{
    [Required]
    public string? RoleName { get; set; }
    public List<GatewayClaim> Claims { get; set; } = new List<GatewayClaim>();
}
