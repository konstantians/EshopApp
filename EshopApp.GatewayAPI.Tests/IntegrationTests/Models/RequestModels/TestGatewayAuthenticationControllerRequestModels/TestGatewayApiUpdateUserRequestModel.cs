namespace EshopApp.GatewayAPI.Tests.IntegrationTests.Models.RequestModels.TestGatewayAuthenticationControllerRequestModels;
internal class TestGatewayApiUpdateUserRequestModel
{
    public TestGatewayAppUser? AppUser { get; set; }
    public bool ActivateEmail { get; set; }
    public string? Password { get; set; }
}
