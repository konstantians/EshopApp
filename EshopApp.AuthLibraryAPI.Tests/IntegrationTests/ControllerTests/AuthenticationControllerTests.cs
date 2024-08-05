using Microsoft.AspNetCore.Mvc.Testing;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;
using System.Net.Http.Json;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
using System.Text.Json;
using FluentAssertions;
using System.Net;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.ResponseModels;
using System.Net.Http.Headers;
using System.Web;

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
    private string? _otherUserId;
    private string? _chosenApiKey;

    [OneTimeSetUp]
    public void OneTimeSetUp()
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
    }

    [Test, Order(1)]
    public async Task Signup_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "konstantinoskinnas@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "bogusPhone";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("X-API-KEY");
    }

    [Test, Order(2)]
    public async Task Signup_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "konstantinoskinnas@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "bogusPhone";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(3)]
    public async Task Signup_ShouldFailAndReturnBadRequest_IfPhoneFieldIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "konstantinoskinnas@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "bogusPhone";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(4)]
    public async Task Signup_ShouldSucceedAndReturnConfirmationToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "kinnaskonstantinos0@gmail.com";
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

    [Test, Order(5)]
    public async Task SignUp_ShouldFailAndReturnBadRequest_IfDuplicateEmail()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "kinnaskonstantinos0@gmail.com";
        testSignUpModel.Password = "Kinas2000!";
        testSignUpModel.PhoneNumber = "6943655624";
        
        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(6)]
    public async Task ConfirmEmail_ShouldFailAndReturnBadRequest_IfUserIdIsInvalidForToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");

        string bogusUserId = "bogusId";
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);
        
        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={bogusUserId}&confirmEmailToken={emailConfirmationToken}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(7)]
    public async Task ConfirmEmail_ShouldFailAndReturnBadRequest_IfEmailConfirmationTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");

        string? userId = _chosenUserId;
        string? bogusEmailConfirmationToken = "bogusEmailConfirmationToken";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&confirmEmailToken={bogusEmailConfirmationToken}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(8)]
    public async Task ConfirmEmail_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");

        string? userId = _chosenUserId;
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&confirmEmailToken={emailConfirmationToken}");
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(9)]
    public async Task GetUserByAccessToken_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/getuserbyaccesstoken");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(10)]
    public async Task GetUserByAccessToken_ShouldReturnUnauthorized_IfAccessTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/getuserbyaccesstoken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(11)]
    public async Task GetUserByAccessToken_ShouldReturnOkAndUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/authentication/getuserbyaccesstoken");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppUser? testAppUser = JsonSerializer.Deserialize<TestAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAppUser.Should().NotBeNull();
        testAppUser!.Id.Should().Be(_chosenUserId);
    }

    [Test, Order(12)]
    public async Task ForgotPassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(13)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailDoesNotBelongToAUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY",_chosenApiKey);
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "bogus@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UserNotFoundWithGivenEmail");
    }

    [Test, Order(14)]
    public async Task ForgotPassword_ShouldReturnBadRequest_IfGivenEmailIsBadlyFormatted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "bogusEmail";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(15)]
    public async Task ForgotPassword_ShouldReturnOkAndResetPasswordToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testForgotPasswordRequestModel = new TestForgotPasswordRequestModel();
        testForgotPasswordRequestModel.Email = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/forgotpassword", testForgotPasswordRequestModel);
        string? passwordResetToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "passwordResetToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        passwordResetToken.Should().NotBeNull();
        _chosenResetPasswordToken = passwordResetToken;
    }

    [Test, Order(16)]
    public async Task ResetPassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        var testResetPasswordRequestModel = new TestResetPasswordModel();
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken;
        testResetPasswordRequestModel.Password = "Kinas2023!";
        testResetPasswordRequestModel.UserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/resetpassword", testResetPasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    //This checks exist, because users should not be able to get to this page without a good reason, since going there is useless for the users if the userId is invalid
    [Test, Order(17)]
    public async Task ResetPassword_ShouldReturnBadRequest_IfResetPasswordTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testResetPasswordRequestModel = new TestResetPasswordModel();
        testResetPasswordRequestModel.Token = "bogusToken";
        testResetPasswordRequestModel.Password = "Kinas2023!";
        testResetPasswordRequestModel.UserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/resetpassword", testResetPasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("UnknownError");
    }

    [Test, Order(18)]
    public async Task ResetPassword_ShouldReturnOkAndAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        var testResetPasswordRequestModel = new TestResetPasswordModel();
        testResetPasswordRequestModel.Token = _chosenResetPasswordToken;
        testResetPasswordRequestModel.Password = "Kinas2023!";
        testResetPasswordRequestModel.UserId = _chosenUserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/resetpassword", testResetPasswordRequestModel);
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
    }

    [Test, Order(19)]
    public async Task SignIn_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testSignInRequestModel.Password = "Kinas2023!";
        testSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(20)]
    public async Task SignIn_ShouldReturnUnauthorized_IfTheCredentialsAreInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Email = "bogusEmail@gmail.com";
        testSignInRequestModel.Password = "bogusPassword";
        testSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(21)]
    public async Task SignIn_ShouldSucceedAndReturnAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testSignInRequestModel.Password = "Kinas2023!";
        testSignInRequestModel.RememberMe = true;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
        _chosenAccessToken = accessToken;
    }

    [Test, Order(22)]
    public async Task ChangePassword_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "Kinas2023!";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(23)]
    public async Task ChangePassword_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "Kinas2023!";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(24)]
    public async Task ChangePassword_ShouldReturnBadRequest_IfGivenOldPasswordDoesNotMuchActualOldPassword()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "bogusOldPassword";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("PasswordMismatchError");
    }
    
    [Test, Order(25)]
    public async Task ChangePassword_ShouldSucceedAndReturnNoContent()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangePasswordRequestModel = new TestChangePasswordRequestModel();
        testChangePasswordRequestModel.OldPassword = "Kinas2023!";
        testChangePasswordRequestModel.NewPassword = "Kinas2020!";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/changepassword", testChangePasswordRequestModel);
       
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test, Order(26)]
    public async Task RequestChangeAccountEmail_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "realag58@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(27)]
    public async Task RequestChangeAccountEmail_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "realag58@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(28)]
    public async Task RequestChangeAccountEmail_ShouldReturnBadRequest_IfTheNewEmailAlreadyExistsInTheSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "realag58@gmail.com";

        var testSignUpRequestModel = new TestSignUpRequestModel();
        testSignUpRequestModel.Email = "realag58@gmail.com";
        testSignUpRequestModel.Password = "Kinas2000!";
        testSignUpRequestModel.PhoneNumber = "6943655624";
        var signUpResponse = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpRequestModel);
        string? signUpResponseBody = await signUpResponse.Content.ReadAsStringAsync();
        TestSignUpResponseModel? signupResponseModel = JsonSerializer.Deserialize<TestSignUpResponseModel>(signUpResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _otherUserId = signupResponseModel!.UserId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("DuplicateEmail");
    }

    [Test, Order(29)]
    public async Task RequestChangeAccountEmail_ShouldReturnOkAndChangeEmailToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "newemail@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        string? changeEmailToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "changeEmailToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        changeEmailToken.Should().NotBeNull();
        _chosenEmailChangeToken = changeEmailToken;
    }

    [Test, Order(30)]
    public async Task ConfirmChangeEmail_ShouldReturnBadRequest_IfEmailChangeTokenIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        string? userId = _chosenUserId;
        string? newEmail = "newemail@gmail.com";
        string? bogusChangeEmailToken = "bogusChangeEmailToken";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmchangeemail?userId={userId}&newEmail={newEmail}&changeEmailToken={bogusChangeEmailToken}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(31)]
    public async Task ConfirmChangeEmail_ShouldReturnOkAndAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        string? userId = _chosenUserId;
        string? newEmail = "newemail@gmail.com";
        string? changeEmailToken = WebUtility.UrlEncode(_chosenEmailChangeToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmchangeemail?userId={userId}&newEmail={newEmail}&changeEmailToken={changeEmailToken}");
        string? accessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "accessToken");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        accessToken.Should().NotBeNull();
    }

    [Test, Order(32)]
    public async Task DeleteAccount_ShouldReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/authentication/deleteaccount");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(33)]
    public async Task DeleteAccount_ShouldReturnUnauthorized_IfInvalidAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bogusToken");
        
        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/authentication/deleteaccount");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(34)]
    public async Task DeleteAccount_ShouldReturnNoContentAndSucceed()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/authentication/deleteaccount");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    //Redirect Testing Area
    [Test, Order(35)]
    public async Task ConfirmEmail_ShouldReturnBadRequest_IfRedirectUrlIsNotTrusted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignUpRequestModel testSignUpModel = new TestSignUpRequestModel();
        testSignUpModel.Email = "other@gmail.com";
        testSignUpModel.Password = "Password123!";
        testSignUpModel.PhoneNumber = "6912345678";
        HttpResponseMessage signUpResponse = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpModel);
        string? signUpResponseBody = await signUpResponse.Content.ReadAsStringAsync();
        TestSignUpResponseModel? signupResponseModel = JsonSerializer.Deserialize<TestSignUpResponseModel>(signUpResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _chosenUserId = signupResponseModel!.UserId;
        _chosenConfirmationEmailToken = signupResponseModel!.ConfirmationToken;
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        
        string? userId = _chosenUserId;
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);
        string? redirectUrl = WebUtility.UrlEncode("https://evil.com/handleRedirect?returnUrl=https://evil.com/home");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&confirmEmailToken={emailConfirmationToken}&redirectUrl={redirectUrl}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("InvalidRedirectUrl");
    }

    [Test, Order(36)]
    public async Task ConfirmEmail_ShouldReturnRedirectConfirmEmailAndReturnAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        string? userId = _chosenUserId;
        string? emailConfirmationToken = WebUtility.UrlEncode(_chosenConfirmationEmailToken);
        string? redirectUrl = WebUtility.UrlEncode("https://localhost:7255/handleRedirect?returnUrl=https://localhost:7255.com/home");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userId}&confirmEmailToken={emailConfirmationToken}&redirectUrl={redirectUrl}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        WebUtility.UrlDecode(response.Headers.Location!.AbsoluteUri).Should().Contain(WebUtility.UrlDecode(redirectUrl));
        response.Headers.Location.Query.Should().Contain("accessToken");
        
        var query = HttpUtility.ParseQueryString(response.Headers.Location.Query);
        _chosenAccessToken = query["accessToken"];
    }

    [Test, Order(37)]
    public async Task ConfirmChangeEmail_ShouldReturnBadRequest_IfRedirectUrlIsNotTrusted()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _chosenAccessToken);
        var testChangeEmailRequestModel = new TestChangeEmailRequestModel();
        testChangeEmailRequestModel.NewEmail = "newotheremail@gmail.com";

        HttpResponseMessage requestEmailChangeResponse = await httpClient.PostAsJsonAsync("api/authentication/requestchangeaccountemail", testChangeEmailRequestModel);
        _chosenEmailChangeToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(requestEmailChangeResponse, "changeEmailToken");

        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        string? userId = _chosenUserId;
        string? newEmail = "newotheremail@gmail.com";
        string? changeEmailToken = WebUtility.UrlEncode(_chosenEmailChangeToken);
        string? redirectUrl = WebUtility.UrlEncode("https://evil.com/handleRedirect?returnUrl=https://evil.com/home");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmchangeemail?userId={userId}&newEmail={newEmail}&changeEmailToken={changeEmailToken}&redirectUrl={redirectUrl}");
        string? errorMessage = await JsonParsingHelperMethods.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be("InvalidRedirectUrl");
    }

    [Test, Order(38)]
    public async Task ConfirmChangeEmail_ShouldReturnRedirectConfirmNewEmailAndReturnAccessToken()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        string? userId = _chosenUserId;
        string? newEmail = "newotheremail@gmail.com";
        string? changeEmailToken = WebUtility.UrlEncode(_chosenEmailChangeToken);
        string? redirectUrl = WebUtility.UrlEncode("https://localhost:7255/handleRedirect?returnUrl=https://localhost:7255/home");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/authentication/confirmchangeemail?userId={userId}&newEmail={newEmail}&changeEmailToken={changeEmailToken}&redirectUrl={redirectUrl}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        WebUtility.UrlDecode(response.Headers.Location!.AbsoluteUri).Should().Contain(WebUtility.UrlDecode(redirectUrl));
        response.Headers.Location.Query.Should().Contain("accessToken");
    }
    
    //Rate Limit Test
    [Test, Order(39)]
    public async Task SignIn_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel();
        testSignInRequestModel.Email = "newotheremail@gmail.com";
        testSignInRequestModel.Password = "Password123!";
        testSignInRequestModel.RememberMe = true;

        var response = new HttpResponseMessage();

        //Act
        for (int i = 0; i < 101; i++)
            response = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        httpClient.Dispose();
        ResetDatabaseHelperMethods.ResetSqlAuthDatabase();
    }
}