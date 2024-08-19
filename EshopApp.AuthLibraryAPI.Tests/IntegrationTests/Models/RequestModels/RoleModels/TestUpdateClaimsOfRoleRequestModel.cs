namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.RoleModels;

internal class TestUpdateClaimsOfRoleRequestModel
{
    public string? RoleId { get; set; }
    public List<TestCustomClaim> NewClaims { get; set; } = new List<TestCustomClaim>();

    public TestUpdateClaimsOfRoleRequestModel() { }

    public TestUpdateClaimsOfRoleRequestModel(string roleId, List<TestCustomClaim> newClaims)
    {
        RoleId = roleId;
        foreach (var newClaim in newClaims ?? Enumerable.Empty<TestCustomClaim>())
            NewClaims.Add(newClaim);
    }
}
