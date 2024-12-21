namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayRoleControllerRequestModels;
internal class TestGatewayApiUpdateClaimsOfRoleRequestModel
{
    public string? RoleId { get; set; }
    public List<TestGatewayClaim> NewClaims { get; set; } = new List<TestGatewayClaim>();
}
