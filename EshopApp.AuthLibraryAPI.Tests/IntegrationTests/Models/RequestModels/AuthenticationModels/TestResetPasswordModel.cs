namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestResetPasswordModel
{
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? Password { get; set; }

    public TestResetPasswordModel() { }

    public TestResetPasswordModel(string userId, string token, string password)
    {
        UserId = userId;
        Token = token;
        Password = password;
    }
}
