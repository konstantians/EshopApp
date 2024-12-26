using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAuthenticationTests.Models.RequestModels;
internal class TestGatewayApiUpdateUserRequestModel
{
    public TestGatewayAppUser? AppUser { get; set; }
    public bool ActivateEmail { get; set; }
    public string? Password { get; set; }
}
