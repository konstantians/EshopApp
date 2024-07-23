namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestResetPasswordModel
{
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? Password { get; set; }
}
