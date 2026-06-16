using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.AuthModels;
using EshopApp.MVC.ViewModels;
using EshopApp.MVC.ViewModels.CartViewModels;
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
    private readonly IConfiguration _configuration;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger, IConfiguration configuration)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
        _configuration = configuration;
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

        return RedirectToAction("Index", "Home");
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

        string rawValue = signInViewModel.UserCartJson ?? "[]";
        List<AddItemToCartViewModel> addItemToCartViewModels;
        //For whatever reason the string is not sent correctly, so yes I give up this fixes it
        if (rawValue.StartsWith("\""))
            rawValue = JsonSerializer.Deserialize<string>(rawValue) ?? "[]";

        addItemToCartViewModels = JsonSerializer.Deserialize<List<AddItemToCartViewModel>>(rawValue, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AddItemToCartViewModel>();

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
        string? accessToken = null;
        Dictionary<string, string>? noErrorResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (noErrorResponseObject != null && noErrorResponseObject.TryGetValue("accessToken", out accessToken))
            SetUpAuthenticationCookie(accessToken, signInViewModel.RememberMe ? 30 : 0);

        //Get users cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        //if the user has no cart is the only way this will return this status code
        if ((int)response.StatusCode == 404)
            return RedirectToAction("Index", "Home");

        validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "SignInAndSignUp", "Account", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        //Now synchronize the items to the cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        foreach (AddItemToCartViewModel addItemToCartViewModel in addItemToCartViewModels)
            addItemToCartViewModel.CartId = user.Cart!.Id;
        response = await httpClient.PostAsJsonAsync("GatewayCart/CartItems/Bulk", addItemToCartViewModels);

        //If any error happens in adding the cart items just send the user to index and add a message that something has went wrong in synchronizing the cart
        if ((int)response.StatusCode >= 400)
            TempData["CartSynchronizationFailure"] = true;

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ExternalSignIn(string identityProviderName, string? urlAfterAuthentication)
    {
        string returnUrl = Url.Action(nameof(HandleExternalSignInRedirect), "Account", new { urlAfterAuthentication }, Request.Scheme)!; //if it is null the query parameter is skipped

        string redirectUrl = $"{new Uri(_configuration["AuthenticationApiBaseUrl"]!)}authentication/externalSignIn?identityProviderName={identityProviderName}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Redirect(redirectUrl);
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

        var responseBody = await response.Content.ReadAsStringAsync();
        //this deals with 4xx errors with non-empty response bodies
        if ((int)response.StatusCode >= 400)
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


        string rawValue = resetPasswordViewModel.UserCartJson ?? "[]";
        List<AddItemToCartViewModel> addItemToCartViewModels;
        //For whatever reason the string is not sent correctly, so yes I give up this fixes it
        if (rawValue.StartsWith("\""))
            rawValue = JsonSerializer.Deserialize<string>(rawValue) ?? "[]";

        addItemToCartViewModels = JsonSerializer.Deserialize<List<AddItemToCartViewModel>>(rawValue, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AddItemToCartViewModel>();

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

        //if status code is 200
        string? accessToken = null;
        Dictionary<string, string>? noErrorResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        if (noErrorResponseObject != null && noErrorResponseObject.TryGetValue("accessToken", out accessToken))
            SetUpAuthenticationCookie(accessToken);

        //Get users cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        //if the user has no cart is the only way this will return this status code
        if ((int)response.StatusCode == 404)
            return RedirectToAction("Index", "Home");

        //this deals with 5xx errors
        validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "SignInAndSignUp", "Account", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        //Now synchronize the items to the cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        foreach (AddItemToCartViewModel addItemToCartViewModel in addItemToCartViewModels)
            addItemToCartViewModel.CartId = user.Cart!.Id;
        response = await httpClient.PostAsJsonAsync("GatewayCart/CartItems/Bulk", addItemToCartViewModels);

        //If any error happens in adding the cart items just send the user to index and add a message that something has went wrong in synchronizing the cart
        if ((int)response.StatusCode >= 400)
            TempData["CartSynchronizationFailure"] = true;

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> EditAccount()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

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
            PhoneNumber = user.PhoneNumber,
            Country = user.Address?.Country,
            City = user.Address?.City,
            PostalCode = user.Address?.PostalCode,
            AddressName = user.Address?.AddressName
        };

        ChangeEmailViewModel changeEmailViewModel = new()
        {
            OldEmail = user.Email
        };

        ChangePasswordViewModel changePasswordViewModel = new()
        {
            UserHasPassword = user.HasPassword
        };

        EditAccountViewModel editAccountModel = new()
        {
            ChangeAccountBasicSettings = changeAccountBasicSettingsViewModel,
            ChangeEmailViewModel = changeEmailViewModel,
            ChangePasswordViewModel = changePasswordViewModel
        };

        return View(editAccountModel);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeBasicAccountSettings(ChangeAccountBasicSettingsViewModel changeAccountBasicSettings)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

        if (!ModelState.IsValid)
            return View("EditAccount", "Account");

        UiAddress uiAddress = new UiAddress();
        uiAddress.Country = changeAccountBasicSettings.Country == "NoValue" || string.IsNullOrEmpty(changeAccountBasicSettings.Country) ? null : changeAccountBasicSettings.Country;
        uiAddress.City = changeAccountBasicSettings.City;
        uiAddress.PostalCode = changeAccountBasicSettings.PostalCode;
        uiAddress.AddressName = changeAccountBasicSettings.AddressName;

        UiUser updatedUser = new()
        {
            FirstName = changeAccountBasicSettings.FirstName,
            LastName = changeAccountBasicSettings.LastName,
            PhoneNumber = changeAccountBasicSettings.PhoneNumber,
            Address = uiAddress
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
            return RedirectToAction("Unauthorized401", "Error");

        if (!ModelState.IsValid)
            return View("EditAccount", "Account");

        var apiChangePasswordModel = new Dictionary<string, string?>
        {
            { "currentPassword", changePasswordViewModel.OldPassword! == "NoPassword123!" ? null : changePasswordViewModel.OldPassword! },
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
            return RedirectToAction("Unauthorized401", "Error");

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

        TempData["ShouldSynchronizeBackEndCart"] = true;
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

        TempData["ShouldSynchronizeBackEndCart"] = true;
        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public async Task<IActionResult> HandleExternalSignInRedirect(string? errorMessage, string? accessToken, string? urlAfterAuthentication)
    {
        if (string.IsNullOrEmpty(accessToken))
            return BadRequest("Invalid access token.");

        SetUpAuthenticationCookie(accessToken);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "Index", "Home");

        //if the user has no cart is the only way this will return this status code
        //so we will create the cart
        if ((int)response.StatusCode == 404)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //we dont have the items here that is fine there are going to be filled by the frontend
            response = await httpClient.PostAsJsonAsync("GatewayCart", new List<AddItemToCartViewModel>());

            TempData["UnknownError"] = false; //fixes a bug
            validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "Index", "Home");
            if (validationResult is not null)
                return validationResult;
        }
        else if (validationResult is not null)
            return validationResult;

        //Tell cart to synchronize
        TempData["ShouldSynchronizeBackEndCart"] = true;

        if (urlAfterAuthentication is not null && !Url.IsLocalUrl(urlAfterAuthentication))
            return Redirect(urlAfterAuthentication!);

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

    //used by confirmation pages
    [HttpGet]
    public IActionResult BasicFrontEndAccessTokenValidation()
    {
        bool result = HelperMethods.BasicTokenValidation(Request);
        return Json(result);
    }

    //used by navbar in the layout
    [HttpGet]
    public IActionResult BasicFrontEndAccessTokenValidationAndClaimExtraction()
    {
        bool result = HelperMethods.BasicTokenValidation(Request);
        if (!result)
            return Json(new
            {
                isValid = false,
                claimValues = new List<object>()
            });
        else
        {
            List<(string, string)> userClaims = HelperMethods.GetClaimsFromToken(Request);
            return Json(new
            {
                isValid = true,
                claimValues = userClaims.Select(userClaim => userClaim.Item2)
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

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
