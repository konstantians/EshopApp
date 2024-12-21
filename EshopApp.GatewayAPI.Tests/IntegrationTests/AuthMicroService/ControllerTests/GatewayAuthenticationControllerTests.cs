using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models.RequestModels.TestGatewayAdminControllerRequestModels;
using EshopApp.GatewayAPI.Tests.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayAuthenticationControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _chosenAccessToken;
    private string? _chosenUserId;
    private string? _chosenUserEmail = "kinnaskonstantinos0@gmail.com";
    private string? _chosenResetPasswordToken;
    private IProcessManagementService _processManagementService;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        //check what happens if microservices are down for signup
        TestGatewayApiSignUpRequestModel testSignUpModel = new TestGatewayApiSignUpRequestModel();
        testSignUpModel.Email = _chosenUserEmail;
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";
        testSignUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", testSignUpModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

        //check what happens if microservices are down for forgotPassword
        var testForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = _chosenUserEmail, ClientUrl = "https://localhost:7070/controller/clientAction" };
        response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/ForgotPassword", testForgotPasswordRequestModel);
        errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

        //check what happens if microservices are down for requestChangeAccountEmail
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "realag58@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };
        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);
        errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

        //check what happens if microservices are down for deleteAccount
        response = await httpClient.DeleteAsync($"api/gatewayAuthentication/deleteaccount");
        errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        errorMessage.Should().NotBeNull().And.Be("OneOrMoreMicroservicesAreUnavailable");

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
    }

    [Test, Order(10)]
    public async Task SignUp_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayApiSignUpRequestModel testSignUpModel = new TestGatewayApiSignUpRequestModel();
        testSignUpModel.Email = _chosenUserEmail;
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";
        testSignUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", testSignUpModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task SignUp_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestGatewayApiSignUpRequestModel testSignUpModel = new TestGatewayApiSignUpRequestModel();
        testSignUpModel.Email = _chosenUserEmail;
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";
        testSignUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", testSignUpModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task Signup_ShouldFailAndReturnBadRequest_IfPhoneFieldIsInvalidOrEmailFieldIsInvalidOrUrlFieldHasInvalidFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayApiSignUpRequestModel invalidPhoneSignUpModel = new TestGatewayApiSignUpRequestModel();
        invalidPhoneSignUpModel.Email = _chosenUserEmail;
        invalidPhoneSignUpModel.PhoneNumber = "bogusPhoneFormat";
        invalidPhoneSignUpModel.Password = "Kinas2000!";
        invalidPhoneSignUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        TestGatewayApiSignUpRequestModel invalidEmailSignUpModel = new TestGatewayApiSignUpRequestModel();
        invalidEmailSignUpModel.Email = "bogusEmailFormat";
        invalidEmailSignUpModel.Email = "6943655624";
        invalidEmailSignUpModel.Password = "Kinas2000!";
        invalidEmailSignUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        TestGatewayApiSignUpRequestModel invalidClientUrlSignUpModel = new TestGatewayApiSignUpRequestModel();
        invalidEmailSignUpModel.Email = _chosenUserEmail;
        invalidEmailSignUpModel.Email = "6943655624";
        invalidEmailSignUpModel.Password = "Kinas2000!";
        invalidEmailSignUpModel.ClientUrl = "bogusUrlFormat";

        //Act
        HttpResponseMessage invalidPhoneResponse = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", invalidPhoneSignUpModel);
        HttpResponseMessage invalidEmailResponse = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", invalidEmailSignUpModel);

        //Assert
        invalidPhoneResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        invalidEmailResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task Signup_ShouldFailAndReturnBadRequest_IfClientUrlContainsDomainThatIsNotTrusted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = _chosenUserEmail;
        signUpModel.PhoneNumber = "6943655624";
        signUpModel.Password = "Kinas2000!";
        signUpModel.ClientUrl = "https://untrustedDomain/controller/clientAction";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OriginForRedirectUrlIsNotTrusted");
    }

    [Test, Order(50)]
    public async Task Signup_ShouldSucceedCreateUserAndSendAccountConfirmationEmail()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = _chosenUserEmail;
        signUpModel.PhoneNumber = "6943655624";
        signUpModel.Password = "Kinas2000!";
        signUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        await Task.Delay(waitTimeInMillisecond);
        string? confirmationEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);


        //Assert
        confirmationEmailLink.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        //the following needs to be inside a try catch block, because it will also try to make the redirect to the client after it hits the confirm email endpoint in the authentication
        //and that is going to fail
        try
        {
            using HttpClient tempHttpClient = new HttpClient();
            await tempHttpClient.GetAsync(confirmationEmailLink);
        }
        catch { }
    }

    [Test, Order(60)]
    public async Task SignUp_ShouldFailAndReturnBadRequest_IfDuplicateEmail()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = _chosenUserEmail;
        signUpModel.PhoneNumber = "6943655624";
        signUpModel.Password = "Kinas2000!";
        signUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(70)]
    public async Task SignIn_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = _chosenUserEmail;
        testGatewayApiSignInRequestModel.Password = "Kinas2023!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(80)]
    public async Task SignIn_ShouldReturnUnauthorized_IfTheCredentialsAreInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "bogus@gmail.com";
        testGatewayApiSignInRequestModel.Password = "BogusPassword!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(90)]
    public async Task SignIn_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = _chosenUserEmail;
        testGatewayApiSignInRequestModel.Password = "Kinas2000!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? accessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(100)]
    public async Task GetUserByAccessToken_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetUserByAccessToken_ShouldReturnUnauthorized_IfAccessTokenIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "bogusToken");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(120)]
    public async Task GetUserByAccessToken_ShouldReturnOkAndUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppUser? testAppUser = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().NotBeNull();
        testAppUser!.Email.Should().Be(_chosenUserEmail);
        _chosenUserId = testAppUser.Id!;
    }

    [Test, Order(130)]
    public async Task ForgotPassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        var testForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = _chosenUserEmail, ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/ForgotPassword", testForgotPasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailDoesNotBelongToAUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = "bogus@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/ForgotPassword", testForgotPasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenEmail");
    }

    [Test, Order(150)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailOrClientUrlIsBadlyFormatted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var invalidEmailFormatForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = "bogusEmailFormat", ClientUrl = "https://localhost:7070/controller/clientAction" };
        var invalidClientUrlFormatForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = _chosenUserEmail, ClientUrl = "bogusUrlFormat" };

        //Act
        HttpResponseMessage invalidEmailResponse = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/forgotpassword", invalidEmailFormatForgotPasswordRequestModel);
        HttpResponseMessage invalidClientUrlResponse = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/forgotpassword", invalidClientUrlFormatForgotPasswordRequestModel);

        //Assert
        invalidEmailResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        invalidClientUrlResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(160)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenClientUrlIsNotTrusted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testGatewayApiForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = _chosenUserEmail, ClientUrl = "https://NotTrustedDomain/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/forgotpassword", testGatewayApiForgotPasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("OriginForRedirectUrlIsNotTrusted");
    }

    [Test, Order(170)]
    public async Task ForgotPassword_ShouldReturnOkAndResetPasswordToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testGatewayApiForgotPasswordRequestModel = new TestGatewayApiForgotPasswordRequestModel() { Email = _chosenUserEmail, ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/forgotpassword", testGatewayApiForgotPasswordRequestModel);
        await Task.Delay(waitTimeInMillisecond);
        string? resetPasswordEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);

        //Assert
        resetPasswordEmailLink.Should().NotBeNull().And.StartWith(testGatewayApiForgotPasswordRequestModel.ClientUrl);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var uri = new Uri(resetPasswordEmailLink!);
        var queryParameters = HttpUtility.ParseQueryString(uri.Query);
        _chosenResetPasswordToken = queryParameters["token"];
    }

    [Test, Order(180)]
    public async Task ResetPassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        var testResetPasswordRequestModel = new TestGatewayApiResetPasswordRequestModel();
        testResetPasswordRequestModel.UserId = _chosenUserId!;
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken!;
        testResetPasswordRequestModel.Password = "Kinas2023!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/resetpassword", testResetPasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(190)]
    public async Task ResetPassword_ShouldReturnBadRequest_IfInvalidRequestModel()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testResetPasswordRequestModel = new TestGatewayApiResetPasswordRequestModel();
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken!;
        testResetPasswordRequestModel.Password = "Kinas2023!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/resetpassword", testResetPasswordRequestModel); //UserId is required and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(200)]
    public async Task ResetPassword_ShouldReturnBadRequest_IfResetPasswordTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testResetPasswordRequestModel = new TestGatewayApiResetPasswordRequestModel();
        testResetPasswordRequestModel.UserId = _chosenUserId!;
        testResetPasswordRequestModel.Token = "bogusResetPasswordToken";
        testResetPasswordRequestModel.Password = "Kinas2023!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/resetpassword", testResetPasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UnknownError");
    }

    [Test, Order(210)]
    public async Task ResetPassword_ShouldReturnOkAndAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testResetPasswordRequestModel = new TestGatewayApiResetPasswordRequestModel();
        testResetPasswordRequestModel.UserId = _chosenUserId!;
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken!;
        testResetPasswordRequestModel.Password = "Kinas2023!";

        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = _chosenUserEmail;
        testGatewayApiSignInRequestModel.Password = "Kinas2023!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/resetpassword", testResetPasswordRequestModel);
        string? accessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");


        HttpResponseMessage signInResponse = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        signInResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(220)]
    public async Task ChangePassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestGatewayApiChangePasswordRequestModel() { CurrentPassword = "Kinas2023!", NewPassword = "Kinas2020!" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/changepassword", testChangePasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(230)]
    public async Task ChangePassword_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "bogusToken");
        var testChangePasswordRequestModel = new TestGatewayApiChangePasswordRequestModel() { CurrentPassword = "Kinas2023!", NewPassword = "Kinas2020!" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/changepassword", testChangePasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(240)]
    public async Task ChangePassword_ShouldReturnBadRequest_IfInvalidRequestModel()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangePasswordRequestModel = new TestGatewayApiChangePasswordRequestModel() { NewPassword = "Kinas2020!" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/changepassword", testChangePasswordRequestModel); //CurrentPassword is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(250)]
    public async Task ChangePassword_ShouldReturnBadRequest_IfGivenCurrentPasswordDoesNotMatchActualCurrentPassword()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangePasswordRequestModel = new TestGatewayApiChangePasswordRequestModel() { CurrentPassword = "BogusCurrentPassword", NewPassword = "Kinas2020!" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/changepassword", testChangePasswordRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("PasswordMismatchError");
    }

    [Test, Order(260)]
    public async Task ChangePassword_ShouldSucceedAndReturnNoContent()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangePasswordRequestModel = new TestGatewayApiChangePasswordRequestModel() { CurrentPassword = "Kinas2023!", NewPassword = "Kinas2020!" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/GatewayAuthentication/changepassword", testChangePasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(270)]
    public async Task RequestChangeAccountEmail_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "realag58@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(280)]
    public async Task RequestChangeAccountEmail_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "bogusToken");
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "realag58@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(290)]
    public async Task RequestChangeAccountEmail_ShouldReturnBadRequest_IfInvalidRequestModel()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { ClientUrl = "https://localhost:7070/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel); //the NewEmail property is required and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(300)]
    public async Task RequestChangeAccountEmail_ShouldReturnBadRequest_IfGivenClientUrlIsNotTrusted()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "realag58@gmail.com", ClientUrl = "https://NotTrustedDomain/controller/clientAction" };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("OriginForRedirectUrlIsNotTrusted");
    }

    [Test, Order(310)]
    public async Task RequestChangeAccountEmail_ShouldReturnBadRequest_IfTheNewEmailAlreadyExistsInTheSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "realag58@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };

        var testGatewayApiSignUpRequestModel = new TestGatewayApiSignUpRequestModel();
        testGatewayApiSignUpRequestModel.Email = "realag58@gmail.com";
        testGatewayApiSignUpRequestModel.Password = "Kinas2000!";
        testGatewayApiSignUpRequestModel.PhoneNumber = "6943655624";
        testGatewayApiSignUpRequestModel.ClientUrl = "https://localhost:7070/controller/clientAction";
        var signUpResponse = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", testGatewayApiSignUpRequestModel);

        await Task.Delay(waitTimeInMillisecond);
        TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true); //just remove the last email from papercut

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(320)]
    public async Task RequestChangeAccountEmail_ShouldReturnOkAndChangeEmailToken()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);
        var testChangeEmailRequestModel = new TestGatewayApiChangeEmailRequestModel() { NewEmail = "newemail@gmail.com", ClientUrl = "https://localhost:7070/controller/clientAction" };

        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = testChangeEmailRequestModel.NewEmail;
        testGatewayApiSignInRequestModel.Password = "Kinas2020!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/requestchangeaccountemail", testChangeEmailRequestModel);

        await Task.Delay(waitTimeInMillisecond);
        string? confirmationEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);


        //Assert
        confirmationEmailLink.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        //the following needs to be inside a try catch block, because it will also try to make the redirect to the client after it hits the confirm email endpoint in the authentication
        //and that is going to fail
        try
        {
            using HttpClient tempHttpClient = new HttpClient();
            await tempHttpClient.GetAsync(confirmationEmailLink);
        }
        catch { }

        HttpResponseMessage signInResponse = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        _chosenAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(signInResponse, "accessToken");
        _chosenUserEmail = testChangeEmailRequestModel.NewEmail;
    }

    [Test, Order(330)]
    public async Task DeleteAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAuthentication/deleteaccount");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(340)]
    public async Task DeleteAccount_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "bogusToken");

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAuthentication/deleteaccount");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(350)]
    public async Task DeleteAccount_ShouldReturnNoContentAndSucceed()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAuthentication/deleteaccount");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetUserByAccessToken_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _chosenAccessToken);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayAuthentication/GetUserByAccessToken");

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
