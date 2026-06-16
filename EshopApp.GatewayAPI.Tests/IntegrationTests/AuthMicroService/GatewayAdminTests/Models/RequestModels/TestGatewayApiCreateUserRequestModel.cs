namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAdminTests.Models.RequestModels;
internal class TestGatewayApiCreateUserRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public bool SendEmailNotification { get; set; }

    public TestGatewayApiCreateUserRequestModel()
    {

    }

    public TestGatewayApiCreateUserRequestModel(string email, string password, string phoneNumber)
    {
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
    }

    public TestGatewayApiCreateUserRequestModel(string email, string password, string phoneNumber, bool sendEmailNotification)
    {
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
        SendEmailNotification = sendEmailNotification;
    }
}
