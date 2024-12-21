using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;
internal class TestGatewayAppRole : IdentityRole
{
    public List<TestGatewayClaim> Claims { get; set; } = new List<TestGatewayClaim>();

    public TestGatewayAppRole() { }

    public TestGatewayAppRole(string givenRoleName) : base(roleName: givenRoleName) { }
}
