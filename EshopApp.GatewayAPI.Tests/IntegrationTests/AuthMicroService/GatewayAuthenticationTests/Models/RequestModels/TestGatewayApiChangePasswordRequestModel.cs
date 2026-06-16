namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAuthenticationTests.Models.RequestModels;
internal class TestGatewayApiChangePasswordRequestModel
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
