namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AdminModels;

internal class TestUpdateUserRequestModel
{
    public TestAppUser? AppUser { get; set; }
    public string? Password { get; set; }
    public bool ActivateEmail { get; set; } = true;

    public TestUpdateUserRequestModel() { }

    public TestUpdateUserRequestModel(TestAppUser testAppUser, string password)
    {
        AppUser = testAppUser;
        Password = password;
    }

    public TestUpdateUserRequestModel(TestAppUser testAppUser, string password, bool activateEmail)
    {
        AppUser = testAppUser;
        Password = password;
        ActivateEmail = activateEmail;
    }
}
