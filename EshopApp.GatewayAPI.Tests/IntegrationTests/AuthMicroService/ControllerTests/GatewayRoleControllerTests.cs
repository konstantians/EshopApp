using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAdminControllerRequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayRoleControllerRequestModels;
using EshopApp.GatewayAPI.Tests.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayRoleControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _userId;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _adminId;
    private string? _userRoleId;
    private string? _managerRoleId;
    private string? _adminRoleId;
    private string? _chosenRoleId;
    private string? _chosenRoleIdToBeUpdated;
    private IProcessManagementService _processManagementService;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        _processManagementService = new ProcessManagementService();
        _processManagementService.BuildAndRunApplication(true, true, true, false, false);

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );

        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();

        //sign up simple user
        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = "kinnaskonstantinos0@gmail.com";
        signUpModel.PhoneNumber = "6943655624";
        signUpModel.Password = "Kinas2016!";
        signUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";
        await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        await Task.Delay(waitTimeInMillisecond);
        string? confirmationEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);
        try
        {
            using HttpClient tempHttpClient = new HttpClient();
            await tempHttpClient.GetAsync(confirmationEmailLink);
        }
        catch { }

        //get user access token and userId
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayApiSignInRequestModel.Password = "Kinas2016!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        _userAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        string? responseBody = await response.Content.ReadAsStringAsync();
        _userId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //get manager access token and userId
        testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "manager@hotmail.com";
        testGatewayApiSignInRequestModel.Password = "CIiyyBRXjTGac7j!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        _managerAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        //get admin access token and adminId
        testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "admin@hotmail.com";
        testGatewayApiSignInRequestModel.Password = "0XfN725l5EwSTIk!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        _adminAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        responseBody = await response.Content.ReadAsStringAsync();
        _adminId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task GetRoles_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(20)]
    public async Task GetRoles_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(30)]
    public async Task GetRoles_ShouldReturnNonElevatedRoles_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestGatewayAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles.Should().HaveCount(2);
    }

    [Test, Order(40)]
    public async Task GetRoles_ShouldReturnOkAndRoles()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestGatewayAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles.Should().HaveCount(3);

        foreach (TestGatewayAppRole testAppRole in testAppRoles!)
        {
            if (testAppRole.Name == "Admin")
                _adminRoleId = testAppRole.Id;
            else if (testAppRole.Name == "Manager")
                _managerRoleId = testAppRole.Id;
            else if (testAppRole.Name == "User")
                _userRoleId = testAppRole.Id;
        }
    }

    [Test, Order(50)]
    public async Task GetRoleById_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyid/{userRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(60)]
    public async Task GetRoleById_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyid/{userRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(70)]
    public async Task GetRoleById_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyid/{adminRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(80)]
    public async Task GetRoleById_ShouldReturnNotFound_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserRoleId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyid/{bogusUserRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(90)]
    public async Task GetRoleById_ShouldReturnOkAndRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userRoleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyid/{userRoleId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppRole? testAppRole = JsonSerializer.Deserialize<TestGatewayAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRole.Should().NotBeNull();
        testAppRole!.Id.Should().Be(userRoleId);
    }

    [Test, Order(100)]
    public async Task GetRoleByName_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyname/{roleName}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(110)]
    public async Task GetRoleByName_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleName = "User";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyname/{roleName}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(120)]
    public async Task GetRoleByName_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyname/{adminRoleName}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(130)]
    public async Task GetRoleByName_ShouldReturnNotFound_IfRoleWithGivenRoleNameWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleName = "bogusRoleName";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyname/{bogusRoleName}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(140)]
    public async Task GetRoleByName_ShouldReturnOkAndRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleName = "Admin";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolebyname/{adminRoleName}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppRole? testAppRole = JsonSerializer.Deserialize<TestGatewayAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRole.Should().NotBeNull();
        testAppRole!.Name.Should().Be(adminRoleName);
    }

    [Test, Order(150)]
    public async Task GetRolesOfUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(160)]
    public async Task GetRolesOfUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task GetRolesOfUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{adminId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(180)]
    public async Task GetRolesOfUser_ShouldReturnBadRequest_IfUserWithGivenUserIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{bogusUserId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(190)]
    public async Task GetRolesOfUser_ShouldReturnOkAndRolesOfUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{userId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestGatewayAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles!.Should().HaveCount(1);
        testAppRoles![0].Claims.Should().NotBeNull();
        testAppRoles[0].Claims.Should().HaveCount(0);
    }

    [Test, Order(200)]
    public async Task CreateRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        List<TestGatewayClaim> customClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageRoles"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestGatewayApiCreateRoleRequestModel() { RoleName = "Manager2", Claims = customClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole", testCreateRoleRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(210)]
    public async Task CreateRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        List<TestGatewayClaim> customClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageRoles"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestGatewayApiCreateRoleRequestModel() { RoleName = "Manager2", Claims = customClaims };


        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole", testCreateRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(220)]
    public async Task CreateRole_ShouldReturnBadRequest_IfDuplicateRoleName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestGatewayClaim> customClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageRoles"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestGatewayApiCreateRoleRequestModel() { RoleName = "Manager", Claims = customClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole", testCreateRoleRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateRole");
    }

    [Test, Order(230)]
    public async Task CreateRole_ShouldCreateRoleAndReturnCreated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestGatewayClaim> customClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageRoles"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testCreateRoleRequestModel = new TestGatewayApiCreateRoleRequestModel() { RoleName = "Manager2", Claims = customClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole", testCreateRoleRequestModel);
        TestGatewayAppRole? createdRole = await response.Content.ReadFromJsonAsync<TestGatewayAppRole>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/gatewayrole/getrolebyid");
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("Manager2");
        createdRole.Claims.Should().HaveCount(2); //that means that the bogus claim was not added as intended

        _chosenRoleId = createdRole.Id;
    }

    [Test, Order(240)]
    public async Task CreateRole_ShouldCreateRoleAndReturnCreated_ButItShouldNotAddElevatedClaims_IfUserDoesNotHaveManageElevatedRolesClaim()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        List<TestGatewayClaim> customClaims = [new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("Permission", "CanManageElevatedRoles")];
        var testCreateRoleRequestModel = new TestGatewayApiCreateRoleRequestModel() { RoleName = "Manager3", Claims = customClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole", testCreateRoleRequestModel);
        var createdRole = await response.Content.ReadFromJsonAsync<TestGatewayAppRole>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/gatewayrole/getrolebyid");
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("Manager3");
        createdRole.Claims.Should().HaveCount(0); //that means that the elevated claims where not added as intented, because the manager does not have any elevated claims
        _chosenRoleIdToBeUpdated = createdRole.Id;
    }

    [Test, Order(250)]
    public async Task DeleteRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{roleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(260)]
    public async Task DeleteRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(270)]
    public async Task DeleteRole_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{adminRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(280)]
    public async Task DeleteUserAccount_ShouldReturnBadRequest_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleId = "bogusRoleId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{bogusRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(290)]
    public async Task DeleteRole_ShouldReturnNoContent()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string chosenRoleId = _chosenRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{chosenRoleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(300)]
    public async Task DeleteRole_ShouldReturnBadRequest_IfElevatedUserTriesToDeleteFundementalRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/{adminRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Fundemental");
    }

    [Test, Order(310)]
    public async Task GetUsersOfRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string roleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{roleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(320)]
    public async Task GetUsersOfRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string roleId = _userRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(330)]
    public async Task GetUsersOfRole_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{adminRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(340)]
    public async Task GetUsersOfRole_ShouldReturnBadRequest_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusRoleId = "bogusBogusId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{bogusRoleId}");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(350)]
    public async Task GetUsersOfRole_ShouldReturnOkAndUsersOfRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{adminRoleId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestGatewayAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(1);
        testAppUsers![0].Id.Should().Be(_adminId);
    }

    [Test, Order(360)]
    public async Task AddRoleToUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _userId, RoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(370)]
    public async Task AddRoleToUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _userId, RoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(380)]
    public async Task AddRoleToUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _userId, RoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(390)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = "bogusUserId", RoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(400)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _userId!, RoleId = "bogusRoleId" };
        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(410)]
    public async Task AddRoleToUser_ShouldReturnBadRequest_IfUserAlreadyInRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _adminId!, RoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasFoundAlreadyInRole");
    }

    [Test, Order(420)]
    public async Task AddRoleToUser_ShouldReturnNoContentAndAddRoleToUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testAddRoleToUserRequestModel = new TestGatewayApiAddRoleToUserRequestModel() { UserId = _userId, RoleId = _adminRoleId };
        string adminRoleId = _adminRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/addroletouser", testAddRoleToUserRequestModel);
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{adminRoleId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestGatewayAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestGatewayAppUser>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(2);
    }

    [Test, Order(430)]
    public async Task ReplaceRoleOfUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _adminRoleId, NewRoleId = _managerRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(440)]
    public async Task ReplaceRoleOfUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken); //this token belongs to the user that is now admin, but it still fails, which is ok because that user can relog back in
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _adminRoleId, NewRoleId = _managerRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(450)]
    public async Task ReplaceRoleOfUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _adminRoleId, NewRoleId = _managerRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(460)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = "bogusUserId", CurrentRoleId = _adminRoleId, NewRoleId = _managerRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(470)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var bogusCurrentRoleIdModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = "bogusCurrentRoleId", NewRoleId = _managerRoleId };
        var bogusNewRoleIdModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _adminRoleId, NewRoleId = "bogusNewRoleId" };

        //Act
        HttpResponseMessage bogusCurrentRoleIdResponse = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", bogusCurrentRoleIdModel);
        string? bogusCurrentRoleIdResponseErrorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(bogusCurrentRoleIdResponse, "errorMessage");
        HttpResponseMessage bogusNewRoleIdResponse = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", bogusCurrentRoleIdModel);
        string? bogusNewRoleIdResponseErrorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(bogusCurrentRoleIdResponse, "errorMessage");

        //Assert
        bogusCurrentRoleIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        bogusCurrentRoleIdResponseErrorMessage.Should().NotBeNull();
        bogusCurrentRoleIdResponseErrorMessage.Should().Be("RoleNotFoundWithGivenId");
        bogusNewRoleIdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        bogusNewRoleIdResponseErrorMessage.Should().NotBeNull();
        bogusNewRoleIdResponseErrorMessage.Should().Be("RoleNotFoundWithGivenId");
    }

    [Test, Order(480)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWasNotFoundInRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _managerRoleId, NewRoleId = _adminRoleId };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel); //user is in roles user and admin not in manager, so that role can not be replaced
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasNotFoundInRole");
    }

    [Test, Order(490)]
    public async Task ReplaceRoleOfUser_ShouldReturnBadRequest_IfUserWasAlreadyInRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _userRoleId!, NewRoleId = _adminRoleId! };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel); //user is in roles user and admin. So he does/can not need to swap user with admin
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasFoundAlreadyInRole");
    }

    [Test, Order(500)]
    public async Task ReplaceRoleOfUser_ShouldReturnNoContentAndAddRoleToUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testReplaceRoleOfUserRequestModel = new TestGatewayApiReplaceRoleOfUserRequestModel() { UserId = _userId, CurrentRoleId = _adminRoleId!, NewRoleId = _managerRoleId! };
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/replaceroleofuser", testReplaceRoleOfUserRequestModel);
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/gatewayRole/getrolesofuser/{userId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestGatewayAppRole>? testAppRoles = JsonSerializer.Deserialize<List<TestGatewayAppRole>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppRoles.Should().NotBeNull();
        testAppRoles!.Should().HaveCount(2);
        testAppRoles.Should().Contain(role => role.Id == _managerRoleId);
        testAppRoles.Should().NotContain(role => role.Id == _adminRoleId);
    }

    [Test, Order(510)]
    public async Task RemoveRoleFromUser_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{userId}/role/{roleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(520)]
    public async Task RemoveRoleFromUser_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{userId}/role/{roleId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(530)]
    public async Task RemoveRoleFromUser_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string userId = _userId!;
        string adminRoleId = _adminRoleId!; //the user currently does not have the admin role, but the check of permissions will happen first

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/RemoveRoleFromUser/{userId}/role/{adminRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(540)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfUserWithGivenUserIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{bogusUserId}/role/{roleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "UserNotFoundWithGivenId");
    }

    [Test, Order(550)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfRoleWithGivenRoleIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string bogusRoleId = "bogusRoleId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{userId}/role/{bogusRoleId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(560)]
    public async Task RemoveRoleFromUser_ShouldReturnBadRequest_IfUserWasNotFoundInRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string idOfRoleThatUserIsNotPart = _adminRoleId!; //user currently not in admin role

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{userId}/role/{idOfRoleThatUserIsNotPart}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserWasNotFoundInRole");
    }

    [Test, Order(570)]
    public async Task RemoveRoleFromUser_ShouldReturnNoContentAndRemoveRoleFromUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string roleId = _managerRoleId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayRole/removerolefromuser/{userId}/role/{roleId}");
        HttpResponseMessage userOfAdminRolesResponse = await httpClient.GetAsync($"api/gatewayRole/getusersofrole/{roleId}");
        string? userOfAdminRolesResponseBody = await userOfAdminRolesResponse.Content.ReadAsStringAsync();
        List<TestGatewayAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestGatewayAppUser>>(userOfAdminRolesResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        userOfAdminRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers!.Should().HaveCount(1);
        testAppUsers.Should().NotContain(user => user.Id == _userId);
    }

    [Test, Order(580)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole/getclaims");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(590)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole/getclaims");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(600)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnNonElevatedClaims_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole/getclaims");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayClaim>? testCustomClaims = JsonSerializer.Deserialize<List<TestGatewayClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCustomClaims.Should().NotBeNull();
        testCustomClaims.Should().NotContain(customClaim => customClaim.Value!.Contains("Elevated"));
    }

    [Test, Order(610)]
    public async Task GetAllUniqueClaimsInSystemAsync_ShouldReturnOkAndUniqueClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayRole/getclaims");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayClaim>? testCustomeClaims = JsonSerializer.Deserialize<List<TestGatewayClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCustomeClaims.Should().NotBeNull();
        testCustomeClaims.Should().HaveCountGreaterThan(4);
    }

    [Test, Order(620)]
    public async Task UpdateClaimsOfRole_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _chosenRoleIdToBeUpdated!, NewClaims = newClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(630)]
    public async Task UpdateClaimsOfRole_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _chosenRoleIdToBeUpdated!, NewClaims = newClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(640)]
    public async Task UpdateClaimsOfRole_ShouldReturnBadRequest_IfRoleWithGivenRoleIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = "bogusRoleId", NewClaims = newClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.EntityNotFoundChecks(response, "RoleNotFoundWithGivenId");
    }

    [Test, Order(650)]
    public async Task UpdateClaimsOfRole_ShouldReturnBadRequest_IfUserTriedToUpdateFundementalRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageRoles"), new TestGatewayClaim("BogusType", "BogusValue")];
        var userFundementalRoleModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _userRoleId, NewClaims = newClaims };
        var adminFundementalRoleModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _adminRoleId, NewClaims = newClaims };

        //Act
        HttpResponseMessage userFundementalRoleResponse = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", userFundementalRoleModel);
        string? userFundementalRoleError = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(userFundementalRoleResponse, "errorMessage");
        HttpResponseMessage adminFundementalRoleResponse = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", adminFundementalRoleModel);
        string? adminFundementalRoleError = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(adminFundementalRoleResponse, "errorMessage");

        //Assert
        userFundementalRoleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        userFundementalRoleError.Should().NotBeNull();
        userFundementalRoleError.Should().Be("CanNotAlterFundementalRole");
        adminFundementalRoleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        adminFundementalRoleError.Should().NotBeNull();
        adminFundementalRoleError.Should().Be("CanNotAlterFundementalRole");
    }

    [Test, Order(660)]
    public async Task UpdateClaimsOfRole_ShouldUpdatedRole_ButItShouldNotAddElevatedClaims_IfUserDoesNotHaveManageElevatedRolesClaim()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _chosenRoleIdToBeUpdated!, NewClaims = newClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(670)]
    public async Task UpdateClaimsOfRole_ShouldUpdatedRole()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        List<TestGatewayClaim> newClaims = [new TestGatewayClaim("Permission", "CanManageUsers"), new TestGatewayClaim("Permission", "CanManageElevatedUsers"), new TestGatewayClaim("BogusType", "BogusValue")];
        var testUpdateClaimsOfRoleRequestModel = new TestGatewayApiUpdateClaimsOfRoleRequestModel() { RoleId = _chosenRoleIdToBeUpdated!, NewClaims = newClaims };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayRole/updateclaimsofrole", testUpdateClaimsOfRoleRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    //Rate Limit Test
    [Test, Order(800)]
    public async Task GetRoles_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync($"api/gatewayRole");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        _processManagementService.TerminateApplication();

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );

        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();

        httpClient.Dispose();
    }
}
