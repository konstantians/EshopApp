using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayRoleTests.Models.RequestModels;
internal class TestGatewayApiUpdateClaimsOfRoleRequestModel
{
    public string? RoleId { get; set; }
    public List<TestGatewayClaim> NewClaims { get; set; } = new List<TestGatewayClaim>();
}
