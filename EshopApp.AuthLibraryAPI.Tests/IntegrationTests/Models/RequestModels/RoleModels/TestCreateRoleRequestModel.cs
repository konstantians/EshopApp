namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.RoleModels;

internal class TestCreateRoleRequestModel
{
    public string? RoleName { get; set; }
    public List<TestCustomClaim> Claims { get; set; } = new List<TestCustomClaim>();

    public TestCreateRoleRequestModel() { }

    public TestCreateRoleRequestModel(string roleName, List<TestCustomClaim> claims)
    {
        RoleName = roleName;
        foreach (var claim in claims)
            Claims.Add(claim);
    }
}