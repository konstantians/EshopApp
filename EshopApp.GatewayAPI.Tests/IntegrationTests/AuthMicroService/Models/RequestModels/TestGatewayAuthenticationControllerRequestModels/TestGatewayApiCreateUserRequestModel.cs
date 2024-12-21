namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAuthenticationControllerRequestModels;
internal class TestGatewayApiCreateUserRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public bool SendEmailNotification { get; set; }

}
