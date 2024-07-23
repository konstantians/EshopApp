using Microsoft.AspNetCore.Mvc.Testing;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;
using System.Net.Http.Json;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
using System.Text.Json;
using FluentAssertions;
using System.Net;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.ResponseModels;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class AuthenticationControllerTests
{
    private HttpClient httpClient;
    private string? _chosenConfirmationEmailToken;
    private string? _chosenUserId;
    private string? _chosenAccessToken;
    private string? _chosenResetPasswordToken;
    private string? _chosenEmailChangeToken;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();

        ResetDatabaseHelperMethods.ResetSqlAuthDatabase();
    }

    [Test, Order(1)]
    public async Task Signup_ShouldFailAndReturnBadRequest_IfPhoneFieldIsInvalid()
    {
        //Arrange
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Username = "konstantinoskinnas@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "bogusPhone";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(2)]
    public async Task Signup_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Username = "kinnaskonstantinos0@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestSignUpResponseModel? signupResponseModel = JsonSerializer.Deserialize<TestSignUpResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        signupResponseModel.Should().NotBeNull();
        signupResponseModel!.UserId.Should().NotBeNull();
        signupResponseModel!.ConfirmationToken.Should().NotBeNull();

        _chosenConfirmationEmailToken = signupResponseModel.ConfirmationToken;
        _chosenUserId = signupResponseModel.UserId;
    }

    [Test, Order(3)]
    public async Task SignUp_ShouldFailAndReturnBadRequest_IfDuplicateEmail()
    {
        //Arrange
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Username = "kinnaskonstantinos0@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(4)]
    public async Task ConfirmEmail_ShouldFailAndReturnBadRequest_IfUserIdIsInvalidForToken()
    {
        //Arrange
        string bogusUserId = "bogusId";
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={bogusUserId}&token={emailConfirmationToken}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(5)]
    public async Task ConfirmEmail_ShouldFailAndReturnBadRequest_IfEmailConfirmationTokenIsInvalid()
    {
        //Arrange
        string? userId = _chosenUserId;
        string? bogusEmailConfirmationToken = "bogusEmailConfirmationToken";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&token={bogusEmailConfirmationToken}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(6)]
    public async Task ConfirmEmail_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        string? userId = _chosenUserId;
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&token={emailConfirmationToken}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(7)]
    public async Task TryGetCurrentUser_ShouldReturnUnauthorized_IfAccessTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/trygetcurrentuser");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(8)]
    public async Task TryGetCurrentUser_ShouldReturnOkAndUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/trygetcurrentuser");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppUser? testAppUser = JsonSerializer.Deserialize<TestAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().Be(_chosenUserId);
    }

    [Test, Order(9)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailDoesNotBelongToAUser()
    {
        //Arrange
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "bogus@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UnkownEmail");
    }

    [Test, Order(10)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailIsBadlyFormatted()
    {
        //Arrange
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "bogusEmail";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(11)]
    public async Task ForgotPassword_ShouldReturnOkAndResetPasswordToken()
    {
        //Arrange
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? passwordResetToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "passwordResetToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        passwordResetToken.Should().NotBeNull();
        _chosenResetPasswordToken = passwordResetToken;
    }

    //This checks exist, because users should not be able to get to this page without a good reason, since going there is useless for the users if the userId is invalid
    [Test, Order(12)]
    public async Task ResetPasswordGet_ShouldReturnBadRequest_IfUserIdIsInvalid()
    {
        //Arrange
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/resetpassword?userId={bogusUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(13)]
    public async Task ResetPasswordGet_ShouldReturnSucceedAndReturnOk()
    {
        //Arrange
        string? chosenUserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/resetpassword?userId={chosenUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test, Order(14)]
    public async Task ResetPasswordPost_ShouldReturnBadRequest_IfResetPasswordTokenIsInvalid()
    {
        //Arrange
        var testResetPasswordRequestModel = new TestResetPasswordModel();
        testResetPasswordRequestModel.Token = "bogusToken";
        testResetPasswordRequestModel.Password = "Kinas2023!";
        testResetPasswordRequestModel.UserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/resetpassword", testResetPasswordRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("InvalidResetTokenForUserId");
    }

    [Test, Order(15)]
    public async Task ResetPasswordPost_ShouldReturnOkAndAccessToken()
    {
        //Arrange
        var testResetPasswordRequestModel = new TestResetPasswordModel();
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken;
        testResetPasswordRequestModel.Password = "Kinas2023!";
        testResetPasswordRequestModel.UserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/resetpassword", testResetPasswordRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
    }

    [Test, Order(16)]
    public async Task SignIn_ShouldReturnUnauthorized_IfTheCredentialsAreInvalid()
    {
        //Arrange
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Username = "bogusUsername";
        testSignInRequestModel.Password = "bogusPassword";
        testSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(17)]
    public async Task SignIn_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Username = "kinnaskonstantinos0@gmail.com";
        testSignInRequestModel.Password = "Kinas2023!";
        testSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(18)]
    public async Task EditAccount_ShouldReturnUnAuthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/editAccount");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(19)]
    public async Task EditAccount_ShouldReturnOkAndUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/editAccount");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppUser? testAppUser = JsonSerializer.Deserialize<TestAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().Be(_chosenUserId);
    }

    [Test, Order(20)]
    public async Task ChangePassword_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "Kinas2023!";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(21)]
    public async Task ChangePassword_ShouldReturnBadRequest_IfGivenOldPasswordDoesNotMuchActualOldPassword()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "bogusOldPassword";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("PasswordMismatchError");
    }
    
    [Test, Order(22)]
    public async Task ChangePassword_ShouldSucceedAndReturnOk()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "Kinas2023!";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);
       
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test, Order(23)]
    public async Task RequestChangeAccountEmail_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "realag58@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(24)]
    public async Task RequestChangeAccountEmail_ShouldReturnBadRequest_IfTheNewEmailAlreadyExistsInTheSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "realag58@gmail.com";

        var testSignUpRequestModel = new TestSignUpRequestModel();
        testSignUpRequestModel.Username = "realag58@gmail.com";
        testSignUpRequestModel.Password = "Kinas2000!";
        testSignUpRequestModel.PhoneNumber = "6943655624";
        await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(25)]
    public async Task RequestChangeAccountEmail_ShouldReturnOkAndChangeEmailToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "newemail@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? changeEmailToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "changeEmailToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        changeEmailToken.Should().NotBeNull();
        _chosenEmailChangeToken = changeEmailToken;
    }

    //TODO think about the internal server error, it should probably return bad request, so maybe change the library itself...
    [Test, Order(26)]
    public async Task ConfirmChangeEmail_ShouldReturnBadRequest_IfEmailChangeTokenIsInvalid()
    {
        //Arrange
        var testConfirmChangeEmailRequestModel = new TestConfirmChangeEmailRequestModel();
        testConfirmChangeEmailRequestModel.UserId = _chosenUserId;
        testConfirmChangeEmailRequestModel.NewEmail = "newemail@gmail.com";
        testConfirmChangeEmailRequestModel.ChangeEmailToken = "bogusToken";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/confirmchangeemail", testConfirmChangeEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(27)]
    public async Task ConfirmChangeEmail_ShouldReturnOkAndAccessToken()
    {
        //Arrange
        var testConfirmChangeEmailRequestModel = new TestConfirmChangeEmailRequestModel();
        testConfirmChangeEmailRequestModel.UserId = _chosenUserId;
        testConfirmChangeEmailRequestModel.NewEmail = "newemail@gmail.com";
        testConfirmChangeEmailRequestModel.ChangeEmailToken = _chosenEmailChangeToken;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/confirmchangeemail", testConfirmChangeEmailRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        httpClient.Dispose();
        ResetDatabaseHelperMethods.ResetSqlAuthDatabase();
    }
}