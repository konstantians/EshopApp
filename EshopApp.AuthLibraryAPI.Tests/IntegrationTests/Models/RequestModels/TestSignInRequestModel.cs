namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestSignInRequestModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
