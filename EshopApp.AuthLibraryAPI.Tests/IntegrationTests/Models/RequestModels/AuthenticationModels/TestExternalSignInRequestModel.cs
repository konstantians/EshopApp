namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestExternalSignInRequestModel
{
    public string? IdentityProviderName { get; set; }
    public string? ReturnUrl { get; set; }
}
