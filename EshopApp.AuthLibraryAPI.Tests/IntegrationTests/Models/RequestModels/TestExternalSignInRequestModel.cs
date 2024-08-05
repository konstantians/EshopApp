namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestExternalSignInRequestModel
{
    public string? IdentityProviderName { get; set; }
    public string? ReturnUrl { get; set; }
}
