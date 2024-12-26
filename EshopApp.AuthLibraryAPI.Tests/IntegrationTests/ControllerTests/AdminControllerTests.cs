using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AdminModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("Konstantinos Kinnas", "kinnaskonstantinos0@gmail.com")]
internal class AdminControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _adminAccessToken;
    private string? _managerAccessToken;
    private string? _userAccessToken;
    private string? _userId;
    private string? _adminId;

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

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );

        (_userId, _adminId, _userAccessToken, _managerAccessToken, _adminAccessToken) = await CommonProcedures.CommonAdminManagerAndUserSetup(httpClient);
    }

    [Test, Order(1)]
    public async Task GetUsers_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/admin");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(2)]
    public async Task GetUsers_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/admin");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(3)]
    public async Task GetUsers_ShouldReturnNonElevatedUsers_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/admin");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers.Should().HaveCount(2);
    }

    [Test, Order(4)]
    public async Task GetUsers_ShouldReturnOkAndUsers()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/admin");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppUser>? testAppUsers = JsonSerializer.Deserialize<List<TestAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers.Should().HaveCount(3);
    }

    [Test, Order(5)]
    public async Task GetUserById_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyid/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(6)]
    public async Task GetUserById_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyid/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(7)]
    public async Task GetUserById_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyid/{adminId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(8)]
    public async Task GetUserById_ShouldReturnNotFound_IfUserWithGivenUserIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyid/{bogusUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(9)]
    public async Task GetUserById_ShouldReturnOkAndUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyid/{userId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppUser? testAppUser = JsonSerializer.Deserialize<TestAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().Be(userId);
    }

    [Test, Order(10)]
    public async Task GetUserByEmail_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyemail/{email}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(11)]
    public async Task GetUserByEmail_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyemail/{email}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(12)]
    public async Task GetUserByEmail_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminEmail = "admin@hotmail.com";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyemail/{adminEmail}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(13)]
    public async Task GetUserByEmail_ShouldReturnNotFound_IfUserWithGivenEmailWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        string bogusEmail = "bogusEmail@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyemail/{bogusEmail}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(14)]
    public async Task GetUserByEmail_ShouldReturnOkAndUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/admin/getuserbyemail/{email}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppUser? testAppUser = JsonSerializer.Deserialize<TestAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Email.Should().Be(email);
    }

    [Test, Order(15)]
    public async Task CreateUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        var testCreateUserRequestModel = new TestCreateUserRequestModel("user@gmail.com", "6911111111", "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/admin", testCreateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(16)]
    public async Task CreateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        var testCreateUserRequestModel = new TestCreateUserRequestModel("user@gmail.com", "6911111111", "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/admin", testCreateUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(17)]
    public async Task CreateUserAccount_ShouldReturnBadRequest_IfEmailOrPhoneIsBadlyFormatted()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var badlyFormattedEmail = new TestCreateUserRequestModel("userBadlyFormattedEmail", "6911111111", "Password123!");
        var badlyFormattedPhone = new TestCreateUserRequestModel("user@gmail.com", "badlyFormattedPhone", "Password123!");

        //Act
        HttpResponseMessage emailResponse = await httpClient.PostAsJsonAsync("api/admin", badlyFormattedEmail);
        HttpResponseMessage phoneResponse = await httpClient.PostAsJsonAsync("api/admin", badlyFormattedPhone);

        //Assert
        emailResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        phoneResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(18)]
    public async Task CreateUserAccount_ShouldReturnBadRequest_IfDuplicateEmail()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var duplicateEmailCreateUserRequestModel = new TestCreateUserRequestModel("kinnaskonstantinos0@gmail.com", "6911111111", "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/admin", duplicateEmailCreateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(19)]
    public async Task CreateUserAccount_ShouldCreateUserAndReturnCreated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testCreateUserRequestModel = new TestCreateUserRequestModel("user@gmail.com", "6911111111", "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/admin", testCreateUserRequestModel);
        var createdUser = await response.Content.ReadFromJsonAsync<TestAppUser>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/admin/getuserbyid");
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(testCreateUserRequestModel.Email);
        createdUser.PhoneNumber.Should().Be(testCreateUserRequestModel.PhoneNumber);
    }

    [Test, Order(20)]
    public async Task UpdateUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestAppUser updatedUser = new TestAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6922222222" };
        var testUpdateUserRequestModel = new TestUpdateUserRequestModel(new TestAppUser(), "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/admin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(21)]
    public async Task UpdateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestAppUser updatedUser = new TestAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6922222222" };
        var testUpdateUserRequestModel = new TestUpdateUserRequestModel(updatedUser, "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/admin", testUpdateUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(22)]
    public async Task UpdateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        TestAppUser updatedUser = new TestAppUser() { Id = _adminId!, Email = "realag58@gmail.com", PhoneNumber = "6922222222" };
        var testUpdateUserRequestModel = new TestUpdateUserRequestModel(updatedUser, "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/admin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(23)]
    public async Task UpdateUserAccount_ShouldReturnNotFound_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestAppUser updatedUser = new TestAppUser() { Id = "bogusUserId", Email = "realag58@gmail.com", PhoneNumber = "6922222222" };

        var testUpdateUserRequestModel = new TestUpdateUserRequestModel(updatedUser, "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/admin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(24)]
    public async Task UpdateUserAccount_ShouldSucceedAndUpdateUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestAppUser updatedUser = new TestAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6922222222" };

        var testUpdateUserRequestModel = new TestUpdateUserRequestModel(updatedUser, "Password123!");

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/admin", testUpdateUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(25)]
    public async Task DeleteUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/admin/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(26)]
    public async Task DeleteUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/admin/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(27)]
    public async Task DeleteUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/admin/{adminId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(28)]
    public async Task DeleteUserAccount_ShouldReturnNotFound_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/admin/{bogusUserId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(29)]
    public async Task DeleteUserAccount_ShouldReturnNoContent()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/admin/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );
    }

}