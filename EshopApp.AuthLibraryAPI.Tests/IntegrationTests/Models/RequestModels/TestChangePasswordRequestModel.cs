namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;

internal class TestChangePasswordRequestModel
{
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}
