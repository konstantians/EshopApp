namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestSignInRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
