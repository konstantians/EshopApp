namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAdminControllerRequestModels;
internal class TestGatewayApiChangePasswordRequestModel
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
