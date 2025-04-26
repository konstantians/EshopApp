using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models;
using EshopApp.MVC.ViewModels;
using EshopApp.MVC.ViewModels.EditAccountViewModels;
using EshopApp.MVC.ViewModels.SignInAndSignUpModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel signUpViewModel)
    {
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            TempData["ShowSignUpForm"] = true;
            return View("SignInAndSignUp");
        }

        var apiSignUpModel = new Dictionary<string, string>
        {
            { "email", signUpViewModel.Email! },
            { "password", signUpViewModel.Password! },
            { "phoneNumber", signUpViewModel.PhoneNumber! },
            { "clientUrl", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/HandleSignUpRedirect" }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/SignUp", apiSignUpModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "SignInAndSignUp", "Account");
        if (validationResult is not null)
        {
            TempData["ShowSignUpForm"] = true;
            return validationResult;
        }

        //if status code is 204
        HttpContext.Session.SetString("showRegisterVerificationView", "true");
        return RedirectToAction("RegisterVerificationEmailMessage", "Account");
    }

    [HttpGet]
    public IActionResult RegisterVerificationEmailMessage()
    {
        string? showRegisterVerificationView = HttpContext.Session.GetString("showRegisterVerificationView");
        if (showRegisterVerificationView is null || showRegisterVerificationView != "true")
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpGet]
    public IActionResult ResetPasswordVerificationEmailMessage()
    {
        string? showResetPasswordVerificationView = HttpContext.Session.GetString("showResetPasswordVerificationView");
        if (showResetPasswordVerificationView is null || showResetPasswordVerificationView != "true")
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpGet]
    public IActionResult ChangeEmailVerificationEmailMessage()
    {
        string? showChangeEmailVerificationView = HttpContext.Session.GetString("showChangeEmailVerificationView");
        if (showChangeEmailVerificationView is null || showChangeEmailVerificationView != "true")
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpGet]
    public IActionResult DeleteAccountVerificationEmailMessage()
    {
        string? showAccountDeletionView = HttpContext.Session.GetString("showAccountDeletionView");
        if (showAccountDeletionView is null || showAccountDeletionView != "true")
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        var response = await httpClient.GetAsync($"Authentication/ConfirmEmail?userId={userId}&token={WebUtility.UrlEncode(token)}");

        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return RedirectToAction("Error");

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return RedirectToAction("Index", "Home", new { FailedAccountActivation = true });

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (responseObject != null && responseObject.TryGetValue("accessToken", out string? accessToken))
            SetUpAuthenticationCookie(accessToken);

        return RedirectToAction("Index", "Home", new { SuccessfulAccountActivation = true });
    }

    [HttpGet]
    public IActionResult SignInAndSignUp()
    {
        if (Request.Cookies["EshopAppAuthenticationCookie"] is not null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInViewModel signInViewModel)
    {
        //if there is an access token just send them to homepage
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        //if the model state is invalid
        if (!ModelState.IsValid)
            return View("SignInAndSignUp");

        var apiSignInModel = new Dictionary<string, string>
        {
            { "email", signInViewModel.Email! },
            { "password", signInViewModel.Password! }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/SignIn", apiSignInModel);
        var responseBody = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "SignInAndSignUp", "Account", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        //if status code is 200
        Dictionary<string, string>? noErrorResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (noErrorResponseObject != null && noErrorResponseObject.TryGetValue("accessToken", out string? accessToken))
            SetUpAuthenticationCookie(accessToken, signInViewModel.RememberMe ? 30 : 0);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
    {
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
            return View();

        var apiForgotPasswordModel = new Dictionary<string, string>
        {
            { "email", forgotPasswordViewModel.RecoveryEmail!},
            { "clientUrl", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Account/ResetPassword" }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/ForgotPassword", apiForgotPasswordModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "SignInAndSignUp", "Account");
        if (validationResult is not null)
            return validationResult;

        HttpContext.Session.SetString("showResetPasswordVerificationView", "true");
        return RedirectToAction("ResetPasswordVerificationEmailMessage", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string userId, string token)
    {
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        var response = await httpClient.GetAsync($"GatewayAuthentication/CheckResetPasswordEligibility?userId={userId}&resetPasswordToken={WebUtility.UrlEncode(token)}");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        //this deals with 4xx errors with empty response bodies
        //In this case the user should not be here and they just get redirected back to home page
        var responseBody = await response.Content.ReadAsStringAsync();
        if ((int)response.StatusCode >= 400 && string.IsNullOrEmpty(responseBody))
            return RedirectToAction("Index", "home");
        //this deals with 4xx errors with non-empty response bodies
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            ViewData[errorMessage ?? "UnknownError"] = true;
            return RedirectToAction("Index", "home");
        }

        UiUser? user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        ViewData["UserEmail"] = user.Email;
        ViewData["UserId"] = userId;
        ViewData["Token"] = token;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewData["UserEmail"] = resetPasswordViewModel.Email;
            ViewData["UserId"] = resetPasswordViewModel.UserId;
            ViewData["Token"] = resetPasswordViewModel.Token;
            return View();
        }

        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("Index", "Home");

        var apiResetPasswordModel = new Dictionary<string, string>
        {
            { "userId", resetPasswordViewModel.UserId! },
            { "token",  resetPasswordViewModel.Token! },
            { "password", resetPasswordViewModel.Password! }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/ResetPassword", apiResetPasswordModel);
        string? responseBody = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "ResetPassword", "Account", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        var responseObjectSuccess = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (responseObjectSuccess != null && responseObjectSuccess.TryGetValue("accessToken", out string? accessToken))
            SetUpAuthenticationCookie(accessToken);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> EditAccount()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        else if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        ChangeAccountBasicSettingsViewModel changeAccountBasicSettingsViewModel = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        ChangeEmailViewModel changeEmailViewModel = new()
        {
            OldEmail = user.Email
        };

        EditAccountViewModel editAccountModel = new()
        {
            ChangeAccountBasicSettings = changeAccountBasicSettingsViewModel,
            ChangeEmailViewModel = changeEmailViewModel
        };

        return View(editAccountModel);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeBasicAccountSettings(ChangeAccountBasicSettingsViewModel changeAccountBasicSettings)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditAccount", "Account");

        UiUser updatedUser = new()
        {
            FirstName = changeAccountBasicSettings.FirstName,
            LastName = changeAccountBasicSettings.LastName,
            PhoneNumber = changeAccountBasicSettings.PhoneNumber
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayAuthentication/UpdateAccount", updatedUser);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditAccount", "Account");
        if (validationResult is not null)
            return validationResult;

        TempData["AccountBasicSettingsChangeSuccess"] = true;
        return RedirectToAction("EditAccount", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditAccount", "Account");

        var apiChangePasswordModel = new Dictionary<string, string>
        {
            { "currentPassword", changePasswordViewModel.OldPassword! },
            { "newPassword", changePasswordViewModel.NewPassword! }
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/ChangePassword", apiChangePasswordModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditAccount", "Account");
        if (validationResult is not null)
            return validationResult;

        TempData["PasswordChangeSuccess"] = true;
        return RedirectToAction("EditAccount", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> RequestChangeAccountEmail(ChangeEmailViewModel changeEmailViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditAccount", "Account");

        var apiChangeEmailModel = new Dictionary<string, string>
        {
            { "newEmail", changeEmailViewModel.NewEmail! },
            { "clientUrl", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/HandleChangeEmailRedirect" }
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/RequestChangeAccountEmail", apiChangeEmailModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditAccount", "Account");
        if (validationResult is not null)
            return validationResult;

        //log out the user
        Response.Cookies.Delete("EshopAppAuthenticationCookie");
        HttpContext.Session.SetString("showChangeEmailVerificationView", "true");
        return RedirectToAction("ChangeEmailVerificationEmailMessage", "Account");
    }

    [HttpPost]
    public IActionResult LogOut()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (string.IsNullOrEmpty(accessToken))
            return View("Error");

        Response.Cookies.Delete("EshopAppAuthenticationCookie");
        return RedirectToAction("Index", "Home");
    }

    //IMPORTANT this only exists to hide the access token from the url
    //under the implementation the access token is still in url history, but at least
    //it is not visible to the user. Also this method takes care of the cart etc(maybe)
    [HttpGet("HandleSignUpRedirect")]
    public IActionResult HandleSignUpRedirect(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return BadRequest("Invalid access token.");

        SetUpAuthenticationCookie(accessToken);

        return RedirectToAction("Index", "Home");
    }

    //IMPORTANT this only exists to hide the access token from the url
    //under the implementation the access token is still in url history, but at least
    //it is not visible to the user. Also this method takes care of the cart etc(maybe)
    [HttpGet("HandleChangeEmailRedirect")]
    public IActionResult HandleChangeEmailRedirect(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return BadRequest("Invalid access token.");

        SetUpAuthenticationCookie(accessToken);

        return RedirectToAction("Index", "Home");
    }

    private void SetUpAuthenticationCookie(string accessToken, int duration = 0)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true
        };

        //no value means that the cookie will be destroyed when the browser closes
        if (duration != 0)
            cookieOptions.Expires = DateTimeOffset.Now.AddDays(30);

        Response.Cookies.Append("EshopAppAuthenticationCookie", accessToken, cookieOptions);
    }

    [HttpGet]
    public IActionResult BasicFrontEndAccessTokenValidation()
    {
        bool result = HelperMethods.BasicTokenValidation(Request);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditAccount");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync("GatewayAuthentication/DeleteAccount");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "SignInAndSignUp", "Account");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        Response.Cookies.Delete("EshopAppAuthenticationCookie");
        HttpContext.Session.SetString("showAccountDeletionView", "true");
        return RedirectToAction("DeleteAccountVerificationEmailMessage", "Account");
    }
}
