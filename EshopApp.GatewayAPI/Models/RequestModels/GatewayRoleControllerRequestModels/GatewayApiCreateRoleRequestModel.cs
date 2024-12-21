using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayRoleControllerRequestModels;

public class GatewayApiCreateRoleRequestModel
{
    [Required]
    public string? RoleName { get; set; }
    public List<GatewayClaim> Claims { get; set; } = new List<GatewayClaim>();
}
