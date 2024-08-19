namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.RoleModels;

internal class TestReplaceRoleOfUserRequestModel
{
    public string? UserId { get; set; }
    public string? CurrentRoleId { get; set; }
    public string? NewRoleId { get; set; }

    public TestReplaceRoleOfUserRequestModel() { }

    public TestReplaceRoleOfUserRequestModel(string userId, string currentRoleId, string newRoleId)
    {
        UserId = userId;
        CurrentRoleId = currentRoleId;
        NewRoleId = newRoleId;
    }
}
