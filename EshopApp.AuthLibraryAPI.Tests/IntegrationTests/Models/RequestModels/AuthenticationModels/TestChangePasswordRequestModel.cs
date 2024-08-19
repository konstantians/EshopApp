namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;

internal class TestChangePasswordRequestModel
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }

    public TestChangePasswordRequestModel() { }
    public TestChangePasswordRequestModel(string currentPassword, string newPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
    }

}
