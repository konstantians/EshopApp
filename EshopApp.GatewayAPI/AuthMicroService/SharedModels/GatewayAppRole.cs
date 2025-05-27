using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.AuthMicroService.Models;

public class GatewayAppRole : IdentityRole
{
    public int UsersInRoleCount { get; set; }

    public List<GatewayClaim> Claims { get; set; } = new List<GatewayClaim>();

    public GatewayAppRole() { }

    public GatewayAppRole(string givenRoleName) : base(roleName: givenRoleName) { }
}
