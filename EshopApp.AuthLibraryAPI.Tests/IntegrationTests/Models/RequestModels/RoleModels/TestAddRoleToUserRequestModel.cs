namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.RoleModels;

internal class TestAddRoleToUserRequestModel
{
    public string? UserId { get; set; }
    public string? RoleId { get; set; }

    public TestAddRoleToUserRequestModel() { }

    public TestAddRoleToUserRequestModel(string userId, string roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
