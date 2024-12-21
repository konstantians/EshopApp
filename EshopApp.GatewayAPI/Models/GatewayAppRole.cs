using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.Models;

public class GatewayAppRole : IdentityRole
{
    public List<GatewayClaim> Claims { get; set; } = new List<GatewayClaim>();

    public GatewayAppRole() { }

    public GatewayAppRole(string givenRoleName) : base(roleName: givenRoleName) { }
}
