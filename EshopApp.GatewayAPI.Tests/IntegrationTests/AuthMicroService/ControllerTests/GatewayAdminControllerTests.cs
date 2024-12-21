using EshopApp.GatewayAPI.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAdminControllerRequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAuthenticationControllerRequestModels;
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
internal class GatewayAdminControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _userId;
    private string? _otherUserId;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _adminId;
    private string? _chosenUserEmail = "kinnaskonstantinos0@gmail.com";
    private IProcessManagementService _processManagementService;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        //check what happens if microservices are down for createUser
        TestGatewayApiCreateUserRequestModel testGatewayApiCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testGatewayApiCreateUserRequestModel.Email = "user@gmail.com";
        testGatewayApiCreateUserRequestModel.Password = "Kinas2000!";
        testGatewayApiCreateUserRequestModel.PhoneNumber = "6943655624";
        testGatewayApiCreateUserRequestModel.SendEmailNotification = true;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testGatewayApiCreateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

        //check what happens if microservices are down for deleteUser
        response = await httpClient.DeleteAsync($"api/gatewayAdmin/userId");
        errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

        //set up microservices and do cleanup
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
        signUpModel.Email = _chosenUserEmail;
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
        testGatewayApiSignInRequestModel.Email = _chosenUserEmail;
        testGatewayApiSignInRequestModel.Password = "Kinas2016!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
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
    public async Task GetUsers_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAdmin");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(20)]
    public async Task GetUsers_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAdmin");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(30)]
    public async Task GetUsers_ShouldReturnNonElevatedUsers_IfUserDoesNotHaveElevatedPermissions()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAdmin");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<GatewayAppUser>? testAppUsers = JsonSerializer.Deserialize<List<GatewayAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers.Should().HaveCount(2);
    }

    [Test, Order(40)]
    public async Task GetUsers_ShouldReturnOkAndUsers()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAdmin");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<GatewayAppUser>? testAppUsers = JsonSerializer.Deserialize<List<GatewayAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUsers.Should().NotBeNull();
        testAppUsers.Should().HaveCount(3);
    }

    [Test, Order(50)]
    public async Task GetUserById_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(60)]
    public async Task GetUserById_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(70)]
    public async Task GetUserById_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToAccessElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{adminId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(80)]
    public async Task GetUserById_ShouldReturnNotFound_IfUserWithGivenUserIdWasNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{bogusUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(90)]
    public async Task GetUserById_ShouldReturnOkAndUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{userId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        GatewayAppUser? testAppUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().Be(userId);
    }

    [Test, Order(100)]
    public async Task CreateUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        var testCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testCreateUserRequestModel.Email = "user@gmail.com";
        testCreateUserRequestModel.PhoneNumber = "6911111111";
        testCreateUserRequestModel.Password = "Password123!";
        testCreateUserRequestModel.SendEmailNotification = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testCreateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(110)]
    public async Task CreateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        var testCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testCreateUserRequestModel.Email = "user@gmail.com";
        testCreateUserRequestModel.PhoneNumber = "6911111111";
        testCreateUserRequestModel.Password = "Password123!";
        testCreateUserRequestModel.SendEmailNotification = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testCreateUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(120)]
    public async Task CreateUserAccount_ShouldReturnBadRequest_IfEmailOrPhoneIsBadlyFormatted()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var badlyFormattedEmailCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        badlyFormattedEmailCreateUserRequestModel.Email = "badEmailFormat";
        badlyFormattedEmailCreateUserRequestModel.PhoneNumber = "6911111111";
        badlyFormattedEmailCreateUserRequestModel.Password = "Password123!";
        badlyFormattedEmailCreateUserRequestModel.SendEmailNotification = true;

        var badlyFormattedPhoneCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        badlyFormattedEmailCreateUserRequestModel.Email = "user@gmail.com";
        badlyFormattedEmailCreateUserRequestModel.PhoneNumber = "badPhoneFormat";
        badlyFormattedEmailCreateUserRequestModel.Password = "Password123!";
        badlyFormattedEmailCreateUserRequestModel.SendEmailNotification = true;

        //Act
        HttpResponseMessage emailResponse = await httpClient.PostAsJsonAsync("api/gatewayAdmin", badlyFormattedEmailCreateUserRequestModel);
        HttpResponseMessage phoneResponse = await httpClient.PostAsJsonAsync("api/gatewayAdmin", badlyFormattedPhoneCreateUserRequestModel);

        //Assert
        emailResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        phoneResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task CreateUserAccount_ShouldReturnBadRequest_IfDuplicateEmail()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testCreateUserRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testCreateUserRequestModel.PhoneNumber = "6911111111";
        testCreateUserRequestModel.Password = "Password123!";
        testCreateUserRequestModel.SendEmailNotification = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testCreateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(140)]
    public async Task CreateUserAccount_ShouldCreateUserSendEmailNotificationEmailAndReturnCreated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testCreateUserRequestModel.Email = "user@gmail.com";
        testCreateUserRequestModel.PhoneNumber = "6911111111";
        testCreateUserRequestModel.Password = "Password123!";
        testCreateUserRequestModel.SendEmailNotification = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testCreateUserRequestModel);
        await Task.Delay(waitTimeInMillisecond);
        string? emailContent = TestUtilitiesLibrary.EmailUtilities.ReadLastEmailFile(deleteEmailFile: true);
        var createdUser = await response.Content.ReadFromJsonAsync<GatewayAppUser>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        emailContent.Should().NotBeNull();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/gatewayadmin/getuserbyid");
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBeNull();
        createdUser.Email.Should().Be(testCreateUserRequestModel.Email);
        createdUser.PhoneNumber.Should().Be(testCreateUserRequestModel.PhoneNumber);
        _otherUserId = createdUser.Id;
    }

    [Test, Order(150)]
    public async Task CreateUserAccount_ShouldCreateUserEmailAndReturnCreatedButNotSendEmailNotification()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        var testCreateUserRequestModel = new TestGatewayApiCreateUserRequestModel();
        testCreateUserRequestModel.Email = "otherUser@gmail.com";
        testCreateUserRequestModel.PhoneNumber = "6922222222";
        testCreateUserRequestModel.Password = "Password123!";
        testCreateUserRequestModel.SendEmailNotification = false; //false is the default, but I am explicitly setting it here

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAdmin", testCreateUserRequestModel);
        var createdUser = await response.Content.ReadFromJsonAsync<GatewayAppUser>();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().ToLower().Should().Contain("api/gatewayadmin/getuserbyid");
        createdUser.Should().NotBeNull();
        createdUser!.Id.Should().NotBeNull();
        createdUser.Email.Should().Be(testCreateUserRequestModel.Email);
        createdUser.PhoneNumber.Should().Be(testCreateUserRequestModel.PhoneNumber);
    }

    [Test, Order(160)]
    public async Task UpdateUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayAppUser updatedUser = new TestGatewayAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6933333333" }; //change email and phone of initial user
        var testUpdateUserRequestModel = new TestGatewayApiUpdateUserRequestModel();
        testUpdateUserRequestModel.AppUser = updatedUser;
        testUpdateUserRequestModel.ActivateEmail = false; //deactivate the account since it is activated
        testUpdateUserRequestModel.Password = "Kinas2020!"; //change the password

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/gatewayAdmin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(170)]
    public async Task UpdateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayAppUser updatedUser = new TestGatewayAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6933333333" }; //change email and phone of initial user
        var testUpdateUserRequestModel = new TestGatewayApiUpdateUserRequestModel();
        testUpdateUserRequestModel.AppUser = updatedUser;
        testUpdateUserRequestModel.ActivateEmail = false; //deactivate the account since it is activated
        testUpdateUserRequestModel.Password = "Kinas2020!"; //change the password

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/gatewayAdmin", testUpdateUserRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(180)]
    public async Task UpdateUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        TestGatewayAppUser updatedUser = new TestGatewayAppUser() { Id = _adminId!, Email = "realag58@gmail.com", PhoneNumber = "6933333333" }; //change email and phone of initial user
        var testUpdateUserRequestModel = new TestGatewayApiUpdateUserRequestModel();
        testUpdateUserRequestModel.AppUser = updatedUser;
        testUpdateUserRequestModel.ActivateEmail = false; //deactivate the account since it is activated
        testUpdateUserRequestModel.Password = "Kinas2020!"; //change the password

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/gatewayAdmin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(190)]
    public async Task UpdateUserAccount_ShouldReturnNotFound_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAppUser updatedUser = new TestGatewayAppUser() { Id = "bogusUserId"!, Email = "realag58@gmail.com", PhoneNumber = "6933333333" }; //change email and phone of initial user
        var testUpdateUserRequestModel = new TestGatewayApiUpdateUserRequestModel();
        testUpdateUserRequestModel.AppUser = updatedUser;
        testUpdateUserRequestModel.ActivateEmail = false; //deactivate the account since it is activated
        testUpdateUserRequestModel.Password = "Kinas2020!"; //change the password

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/gatewayAdmin", testUpdateUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(200)]
    public async Task UpdateUserAccount_ShouldSucceedAndUpdateUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAppUser updatedUser = new TestGatewayAppUser() { Id = _userId!, Email = "realag58@gmail.com", PhoneNumber = "6933333333" }; //change email and phone of initial user
        var testUpdateUserRequestModel = new TestGatewayApiUpdateUserRequestModel();
        testUpdateUserRequestModel.AppUser = updatedUser;
        testUpdateUserRequestModel.ActivateEmail = false; //deactivate the account since it is activated
        testUpdateUserRequestModel.Password = "Kinas2020!"; //change the password

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/gatewayAdmin", testUpdateUserRequestModel);

        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{_userId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        GatewayAppUser? updatedAppUser = JsonSerializer.Deserialize<GatewayAppUser>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedAppUser!.Email.Should().Be(updatedUser.Email);
        updatedAppUser!.PhoneNumber.Should().Be(updatedUser.PhoneNumber);
        updatedUser.EmailConfirmed.Should().Be(testUpdateUserRequestModel.ActivateEmail);
        _chosenUserEmail = updatedUser.Email;
    }

    [Test, Order(210)]
    public async Task DeleteUserAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string userId = _userId!;
        string userEmail = _chosenUserEmail!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}/userEmail/{userEmail}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        TestUtilitiesLibrary.CommonTestProcedures.ApiKeyIsMissingChecks(response, "Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string userId = _userId!;
        string userEmail = _chosenUserEmail!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}/userEmail/{userEmail}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(230)]
    public async Task DeleteUserAccount_ShouldReturnForbidden_IfUserDoesNotHaveElevatedPermissionsToEditElevatedUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        string adminId = _adminId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{adminId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Insufficient");
    }

    [Test, Order(240)]
    public async Task DeleteUserAccount_ShouldReturnNotFound_IfUserWithGivenIdIsNotFound()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{bogusUserId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenId");
    }

    [Test, Order(250)]
    public async Task DeleteUserAccount_ShouldReturnNoContentAndSendNotificationEmail()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _userId!;
        string userEmail = _chosenUserEmail!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}/userEmail/{userEmail}");
        await Task.Delay(waitTimeInMillisecond);
        string? emailContent = TestUtilitiesLibrary.EmailUtilities.ReadLastEmailFile(deleteEmailFile: true);

        HttpResponseMessage secondDeleteResponse = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}/userEmail/{userEmail}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        emailContent.Should().NotBeNull();
    }

    [Test, Order(260)]
    public async Task DeleteUserAccount_ShouldReturnNoContentButNotSendNotificationEmail()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string userId = _otherUserId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}");
        await Task.Delay(waitTimeInMillisecond);
        string? emailContent = TestUtilitiesLibrary.EmailUtilities.ReadLastEmailFile(deleteEmailFile: true);

        HttpResponseMessage secondDeleteResponse = await httpClient.DeleteAsync($"api/gatewayAdmin/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        emailContent.Should().BeNull();
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetUsers_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/GatewayAdmin");

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
