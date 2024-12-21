namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayRoleControllerRequestModels;
internal class TestGatewayApiCreateRoleRequestModel
{
    public string? RoleName { get; set; }
    public List<TestGatewayClaim> Claims { get; set; } = new List<TestGatewayClaim>();
}
