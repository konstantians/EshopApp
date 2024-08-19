namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestConfirmChangeEmailRequestModel
{
    public string? UserId { get; set; }
    public string? NewEmail { get; set; }
    public string? ChangeEmailToken { get; set; }

    public TestConfirmChangeEmailRequestModel() { }

    public TestConfirmChangeEmailRequestModel(string userId, string newEmail, string changeEmailToken)
    {
        UserId = userId;
        NewEmail = newEmail;
        ChangeEmailToken = changeEmailToken;
    }
}
