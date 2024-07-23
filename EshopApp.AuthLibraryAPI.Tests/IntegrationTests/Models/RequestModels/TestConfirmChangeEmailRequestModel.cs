namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestConfirmChangeEmailRequestModel
{
    public string? UserId { get; set; }
    public string? NewEmail { get; set; }
    public string? ChangeEmailToken { get; set; }
}
