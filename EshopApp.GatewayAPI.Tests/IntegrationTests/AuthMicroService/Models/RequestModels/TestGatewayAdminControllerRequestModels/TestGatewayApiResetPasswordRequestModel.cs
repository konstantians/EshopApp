namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAdminControllerRequestModels;
internal class TestGatewayApiResetPasswordRequestModel
{
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
}
