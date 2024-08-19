namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestSignUpRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }

    public TestSignUpRequestModel() { }

    public TestSignUpRequestModel(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public TestSignUpRequestModel(string email, string password, string phoneNumber)
    {
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
    }
}
