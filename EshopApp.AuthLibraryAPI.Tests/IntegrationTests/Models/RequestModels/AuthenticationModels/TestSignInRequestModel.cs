namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestSignInRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }

    public TestSignInRequestModel() { }

    public TestSignInRequestModel(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public TestSignInRequestModel(string email, string password, bool rememberMe)
    {
        Email = email;
        Password = password;
        RememberMe = rememberMe;
    }
}
