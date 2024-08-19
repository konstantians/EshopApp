
namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AdminModels;

internal class TestCreateUserRequestModel
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    public TestCreateUserRequestModel() { }

    public TestCreateUserRequestModel(string email, string phoneNumber, string password)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        Password = password;
    }

    public TestCreateUserRequestModel(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
