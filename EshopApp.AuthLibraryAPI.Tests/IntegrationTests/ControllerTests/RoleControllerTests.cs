using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models;
using System.Net.Http.Json;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.RoleModels;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("Konstantinos Kinnas", "kinnaskonstantinos0@gmail.com")]
internal class RoleControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _adminAccessToken;
    private string? _managerAccessToken;
    private string? _userAccessToken;
    private string? _userId;
    private string? _adminId;
    private string? _userRoleId;  
    private string? _managerRoleId;
    private string? _adminRoleId;
    private string? _chosenRoleId;
    private string? _chosenRoleIdToBeUpdated;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");

        ResetDatabaseHelperMethods.ResetSqlAuthDatabase();

        (_userId, _adminId, _userAccessToken, _managerAccessToken, _adminAccessToken) = await CommonProcedures.commonAdminManagerAndUserSetup(httpClient);
    }

    [Test, Order(1)]
    public async Task GetRoles_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(2)]
    public async Task GetRoles_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(3)]
    public async Task GetRoles_ShouldReturnNonElevatedRoles_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles.Should().HaveCount(2);
    }

    [Test, Order(4)]
    public async Task GetRoles_ShouldReturnOkAndRoles()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles.Should().HaveCount(3);

        foreach (TestAppRole testAppRole in testAppRoles!)
        {
            if (testAppRole.Name == "Admin")
                _adminRoleId = testAppRole.Id;
            else if (testAppRole.Name == "Manager")
                _managerRoleId = testAppRole.Id;
            else if (testAppRole.Name == "User")
                _userRoleId = testAppRole.Id;
        }
    }

    [Test, Order(5)]
    public async Task GetRoleById_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyid/{userRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(6)]
    public async Task GetRoleById_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyid/{userRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(7)]
    public async Task GetRoleById_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyid/{adminRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(8)]
    public async Task GetRoleById_ShouldReturnNotFound_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserRoleId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyid/{bogusUserRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(9)]
    public async Task GetRoleById_ShouldReturnOkAndRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyid/{userRoleId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppRole? testAppRole = JsonSerializer.Deserialize<TestAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRole.Should().NotBeNull();
        testAppRole!.Id.Should().Be(userRoleId);
    }

    [Test, Order(10)]
    public async Task GetRoleByName_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyname/{roleName}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(11)]
    public async Task GetRoleByName_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleName = "User";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyname/{roleName}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(12)]
    public async Task GetRoleByName_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyname/{adminRoleName}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(13)]
    public async Task GetRoleByName_ShouldReturnNotFound_IfRoleWithGivenRoleNameWasNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleName = "bogusRoleName";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyname/{bogusRoleName}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(14)]
    public async Task GetRoleByName_ShouldReturnOkAndRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolebyname/{adminRoleName}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppRole? testAppRole = JsonSerializer.Deserialize<TestAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRole.Should().NotBeNull();
        testAppRole!.Name.Should().Be(adminRoleName);
    }

    [Test, Order(15)]
    public async Task GetRolesOfUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolesofuser/{userId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(16)]
    public async Task GetRolesOfUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolesofuser/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(17)]
    public async Task GetRolesOfUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedUser()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolesofuser/{adminId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(18)]
    public async Task GetRolesOfUser_ShouldReturnBadRequest_IfUserWithGivenUserIdWasNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolesofuser/{bogusUserId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(19)]
    public async Task GetRolesOfUser_ShouldReturnOkAndRolesOfUser()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getrolesofuser/{userId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles!.Should().HaveCount(1);
        testAppRoles![0].Claims.Should().NotBeNull();
        testAppRoles[0].Claims.Should().HaveCount(0);
    }

    [Test, Order(20)]
    public async Task CreateRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageRoles"), new TestCustomClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestCreateRoleRequestModel("Manager2", customClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role", testCreateRoleRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(21)]
    public async Task CreateRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageRoles"), new TestCustomClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestCreateRoleRequestModel("Manager2", customClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role", testCreateRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(22)]
    public async Task CreateRole_ShouldReturnBadRequest_IfDuplicateRoleName()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageRoles"), new TestCustomClaim("BogusType", "BogusValue")];
        var duplicateRoleNameTestCreateRoleRequestModel = new TestCreateRoleRequestModel("Manager", customClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role", duplicateRoleNameTestCreateRoleRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateRole");
    }

    [Test, Order(23)]
    public async Task CreateRole_ShouldCreateRoleAndReturnCreated()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageRoles"), new TestCustomClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestCreateRoleRequestModel("Manager2", customClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role", testCreateRoleRequestModel);
        var createdRole = await response.Content.ReadFromJsonAsync<TestAppRole>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/role/getrolebyid");
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("Manager2");
        createdRole.Claims.Should().HaveCount(2); //that means that the bogus claim was not added as intended

        _chosenRoleId = createdRole.Id;
    }

    [Test, Order(24)]
    public async Task CreateRole_ShouldCreateRoleAndReturnCreated_ButItShouldNotAddElevatedClaims_IfUserDoesNotHaveManageElevatedRolesClaim()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("Permission", "CanManageElevatedRoles")];
        var testCreateRoleRequestModel = new TestCreateRoleRequestModel("Manager3", customClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role", testCreateRoleRequestModel);
        var createdRole = await response.Content.ReadFromJsonAsync<TestAppRole>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/role/getrolebyid");
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("Manager3");
        createdRole.Claims.Should().HaveCount(0); //that means that the elevated claims where not added as intented, because the manager does not have any elevated claims
        _chosenRoleIdToBeUpdated = createdRole.Id;
    }

    [Test, Order(25)]
    public async Task DeleteRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{roleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(26)]
    public async Task DeleteRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(27)]
    public async Task DeleteRole_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{adminRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(28)]
    public async Task DeleteUserAccount_ShouldReturnBadRequest_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleId = "bogusRoleId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{bogusRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(29)]
    public async Task DeleteRole_ShouldReturnNoContent()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string chosenRoleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{chosenRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(30)]
    public async Task DeleteRole_ShouldReturnBadRequest_IfElevatedUserTriesToDeleteFundementalRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/{adminRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Fundemental");
    }

    [Test, Order(31)]
    public async Task GetUsersOfRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getusersofrole/{roleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(32)]
    public async Task GetUsersOfRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getusersofrole/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(33)]
    public async Task GetUsersOfRole_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getusersofrole/{adminRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(34)]
    public async Task GetUsersOfRole_ShouldReturnBadRequest_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleId = "bogusBogusId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getusersofrole/{bogusRoleId}");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(35)]
    public async Task GetUsersOfRole_ShouldReturnOkAndUsersOfRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/role/getusersofrole/{adminRoleId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(1);
        testAppUsers![0].Id.Should().Be(_adminId);
    }

    [Test, Order(36)]
    public async Task AddRoleToUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_userId!, _adminRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(37)]
    public async Task AddRoleToUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_userId!, _adminRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(38)]
    public async Task AddRoleToUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_userId!, _adminRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(39)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel("bogusUserId", _adminRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(40)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_userId!, "bogusRoleId");

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(41)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfUserAlreadyInRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_adminId!, _adminRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasFoundAlreadyInRole");
    }

    [Test, Order(42)]
    public async Task AddRoleToUser_ShouldReturnNoContentAndAddRoleToUser()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestAddRoleToUserRequestModel(_userId!, _adminRoleId!);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/addroletouser", testAddRoleToUserRequestModel);
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/role/getusersofrole/{adminRoleId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestAppUser>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(2);
    }

    [Test, Order(43)]
    public async Task ReplaceRoleOfUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _adminRoleId!, _managerRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(44)]
    public async Task ReplaceRoleOfUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _adminRoleId!, _managerRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(45)]
    public async Task ReplaceRoleOfUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _adminRoleId!, _managerRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(46)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel("bogusUserId", _adminRoleId!, _managerRoleId!);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(47)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var bogusCurrentRoleIdModel = new TestReplaceRoleOfUserRequestModel(_userId!, "bogusCurrentRoleId", _managerRoleId!);
        var bogusNewRoleIdModel = new TestReplaceRoleOfUserRequestModel(_userId!, _adminRoleId!, "bogusNewRoleId");

        //Act
        HttpResponseMessage bogusCurrentRoleIdResponse = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", bogusCurrentRoleIdModel);
        string? bogusCurrentRoleIdResponseErrorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(bogusCurrentRoleIdResponse, "errorMessage");
        HttpResponseMessage bogusNewRoleIdResponse = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", bogusCurrentRoleIdModel);
        string? bogusNewRoleIdResponseErrorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(bogusCurrentRoleIdResponse, "errorMessage");

        //Assert
        bogusCurrentRoleIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        bogusCurrentRoleIdResponseErrorMessage.Should().NotBeNull();
        bogusCurrentRoleIdResponseErrorMessage.Should().Be("RoleNotFoundWithGivenId");
        bogusNewRoleIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        bogusNewRoleIdResponseErrorMessage.Should().NotBeNull();
        bogusNewRoleIdResponseErrorMessage.Should().Be("RoleNotFoundWithGivenId");
    }

    [Test, Order(48)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWasNotFoundInRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _managerRoleId!, _adminRoleId!); //user is in roles user and admin not in manager, so that role can not be replaced

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasNotFoundInRole");        
    }

    [Test, Order(49)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWasAlreadyInRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _userRoleId!, _adminRoleId!); //user is in roles user and admin. So he does not need to swap user with admin

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasFoundAlreadyInRole");
    }

    [Test, Order(50)]
    public async Task ReplaceRoleOfUser_ShouldReturnNoContentAndAddRoleToUser()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestReplaceRoleOfUserRequestModel(_userId!, _adminRoleId!, _managerRoleId!);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/role/getrolesofuser/{userId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestAppRole>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles!.Should().HaveCount(2);
        testAppRoles.Should().Contain(role => role.Id == _managerRoleId);
        testAppRoles.Should().NotContain(role => role.Id == _adminRoleId);
    }

    [Test, Order(51)]
    public async Task RemoveRoleFromUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{userId}/role/{roleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(52)]
    public async Task RemoveRoleFromUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{userId}/role/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(53)]
    public async Task RemoveRoleFromUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string userId = _userId!;
        string adminRoleId = _adminRoleId!; //the user currently does not have the admin role, but the check of permissions will happen first

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/RemoveRoleFromUser/{userId}/role/{adminRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(54)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{bogusUserId}/role/{roleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(55)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string bogusRoleId = "bogusRoleId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{userId}/role/{bogusRoleId}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(56)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfUserWasNotFoundInRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string idOfRoleThatUserIsNotPart = _adminRoleId!; //user currently not in admin role

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{userId}/role/{idOfRoleThatUserIsNotPart}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasNotFoundInRole");
    }

    [Test, Order(57)]
    public async Task RemoveRoleFromUser_ShouldReturnNoContentAndRemoveRoleFromUser()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/role/removerolefromuser/{userId}/role/{roleId}");
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/role/getusersofrole/{roleId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestAppUser>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(1);
        testAppUsers.Should().NotContain(user => user.Id == _userId);
    }

    [Test, Order(58)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role/getclaims");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(59)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role/getclaims");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnNonElevatedClaims_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role/getclaims");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestCustomClaim>? testCustomClaims = JsonSerializer.Deserialize<List<TestCustomClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCustomClaims.Should().NotBeNull();
        testCustomClaims.Should().NotContain(customClaim => customClaim.Value!.Contains("Elevated"));
    }

    [Test, Order(61)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnOkAndUniqueClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/role/getclaims");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestCustomClaim>? testCustomeClaims = JsonSerializer.Deserialize<List<TestCustomClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCustomeClaims.Should().NotBeNull();
        testCustomeClaims.Should().HaveCountGreaterThan(4);
    }

    [Test, Order(62)]
    public async Task UpdateClaimsOfRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        List<TestCustomClaim> newClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestUpdateClaimsOfRoleRequestModel(_chosenRoleIdToBeUpdated!, newClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(63)]
    public async Task UpdateClaimsOfRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        List<TestCustomClaim> newClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestUpdateClaimsOfRoleRequestModel(_chosenRoleIdToBeUpdated!, newClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(64)]
    public async Task UpdateClaimsOfRole_ShouldReturnBadRequest_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestCustomClaim> newClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestUpdateClaimsOfRoleRequestModel("bogusRoleId", newClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        CommonProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(65)]
    public async Task UpdateClaimsOfRole_ShouldReturnBadRequest_IfUserTriedToUpdateFundementalRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestCustomClaim> customClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageRoles"), new TestCustomClaim("BogusType", "BogusValue")];
        var userFundementalRoleModel = new TestUpdateClaimsOfRoleRequestModel(_userRoleId!, customClaims);
        var adminFundementalRoleModel = new TestUpdateClaimsOfRoleRequestModel(_adminRoleId!, customClaims);

        //Act
        HttpResponseMessage userFundementalRoleResponse = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", userFundementalRoleModel);
        string? userFundementalRoleError = await JsonParsingHelperMethods.GetSingleStringValueFromBody(userFundementalRoleResponse, "errorMessage");
        HttpResponseMessage adminFundementalRoleResponse = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", adminFundementalRoleModel);
        string? adminFundementalRoleError = await JsonParsingHelperMethods.GetSingleStringValueFromBody(adminFundementalRoleResponse, "errorMessage");

        //Assert
        userFundementalRoleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        userFundementalRoleError.Should().NotBeNull();
        userFundementalRoleError.Should().Be("CanNotAlterFundementalRole");
        adminFundementalRoleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        adminFundementalRoleError.Should().NotBeNull();
        adminFundementalRoleError.Should().Be("CanNotAlterFundementalRole");
    }

    [Test, Order(66)]
    public async Task UpdateClaimsOfRole_ShouldCreateRoleAndReturnUpdatedRole_ButItShouldNotAddElevatedClaims_IfUserDoesNotHaveManageElevatedRolesClaim()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        List<TestCustomClaim> newClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestUpdateClaimsOfRoleRequestModel(_chosenRoleIdToBeUpdated!, newClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppRole? appRole = JsonSerializer.Deserialize<TestAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        appRole.Should().NotBeNull();
        appRole!.Claims.Should().HaveCount(1);
        appRole!.Claims.Should().Contain(claim => claim.Type == "Permission" && claim.Value == "CanManageUsers");
    }

    [Test, Order(67)]
    public async Task UpdateClaimsOfRole_ShouldCreateRoleAndReturnUpdatedRole()
    {
        //Arrange
        CommonProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestCustomClaim> newClaims = [new TestCustomClaim("Permission", "CanManageUsers"), new TestCustomClaim("Permission", "CanManageElevatedUsers"), new TestCustomClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestUpdateClaimsOfRoleRequestModel(_chosenRoleIdToBeUpdated!, newClaims);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/role/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppRole? appRole = JsonSerializer.Deserialize<TestAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        appRole.Should().NotBeNull();
        appRole!.Claims.Should().HaveCount(2);
        appRole!.Claims.Should().Contain(claim => claim.Type == "Permission" && claim.Value == "CanManageUsers");
        appRole!.Claims.Should().Contain(claim => claim.Type == "Permission" && claim.Value == "CanManageElevatedUsers");
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        ResetDatabaseHelperMethods.ResetSqlAuthDatabase();
    }
}