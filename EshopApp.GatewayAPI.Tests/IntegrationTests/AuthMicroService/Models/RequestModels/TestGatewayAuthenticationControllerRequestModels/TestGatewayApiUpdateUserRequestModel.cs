using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAuthenticationControllerRequestModels;
internal class TestGatewayApiUpdateUserRequestModel
{
    public TestGatewayAppUser? AppUser { get; set; }
    public bool ActivateEmail { get; set; }
    public string? Password { get; set; }
}
