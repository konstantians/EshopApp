namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayRoleControllerRequestModels;
internal class TestGatewayApiReplaceRoleOfUserRequestModel
{
    public string? UserId { get; set; }
    public string? CurrentRoleId { get; set; }
    public string? NewRoleId { get; set; }
}
