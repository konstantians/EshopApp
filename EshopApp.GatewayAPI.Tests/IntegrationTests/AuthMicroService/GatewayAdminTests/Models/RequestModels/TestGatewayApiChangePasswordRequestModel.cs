namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAdminTests.Models.RequestModels;
internal class TestGatewayApiChangePasswordRequestModel
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
