using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayRoleTests.Models.RequestModels;
internal class TestGatewayApiCreateRoleRequestModel
{
    public string? RoleName { get; set; }
    public List<TestGatewayClaim> Claims { get; set; } = new List<TestGatewayClaim>();
}
