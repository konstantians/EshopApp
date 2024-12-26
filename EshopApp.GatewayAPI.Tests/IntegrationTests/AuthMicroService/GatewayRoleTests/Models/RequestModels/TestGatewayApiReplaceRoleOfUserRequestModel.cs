namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayRoleTests.Models.RequestModels;
internal class TestGatewayApiReplaceRoleOfUserRequestModel
{
    public string? UserId { get; set; }
    public string? CurrentRoleId { get; set; }
    public string? NewRoleId { get; set; }
}
