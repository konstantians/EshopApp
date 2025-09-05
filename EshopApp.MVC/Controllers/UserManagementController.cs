using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models;
using EshopApp.MVC.ViewModels;
using EshopApp.MVC.ViewModels.EditUserViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class UserManagementController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AccountController> _logger;

    public UserManagementController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAdmin");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        List<UiUser> users = JsonSerializer.Deserialize<List<UiUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUserAccount(string userId, string userEmail)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("ManageUsers");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayAdmin/{userId}/userEmail/{userEmail}");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageUsers", "UserManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["UserDeletionSuccess"] = true;
        return RedirectToAction("ManageUsers", "UserManagement");
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/ClaimType/Permission/ClaimValue/CanManageUsers");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayRole");
        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        //this means the user can't access roles, but they could access users
        if ((int)response.StatusCode >= 403)
        {
            ViewData["UserRoles"] = null;
            return View();
        }
        var responseBody = await response.Content.ReadAsStringAsync();
        List<UiRole> roles = JsonSerializer.Deserialize<List<UiRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        ViewData["UserRoles"] = roles;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel createUserViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("CreateUser");

        var apiCreateUserModel = new Dictionary<string, string?>
        {
            { "email", createUserViewModel.Email! },
            { "password", createUserViewModel.Password! },
            { "phoneNumber", createUserViewModel.PhoneNumber },
            { "firstName", createUserViewModel.FirstName },
            { "lastName", createUserViewModel.LastName }
        }; //there is an option that allows the user to send an email to the created email account, so think about that

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayAdmin", apiCreateUserModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "CreateUser", "UserManagement");
        if (validationResult is not null)
            return validationResult;

        if (!string.IsNullOrEmpty(createUserViewModel.UserRoleId) && !string.IsNullOrEmpty(createUserViewModel.TheIdOfTheRoleUser) && createUserViewModel.UserRoleId != createUserViewModel.TheIdOfTheRoleUser)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            UiUser createdUser = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            var apiReplaceRoleOfUserModel = new Dictionary<string, string>
            {
                { "userId", createdUser.Id! },
                { "currentRoleId", createUserViewModel.TheIdOfTheRoleUser! },
                { "newRoleId", createUserViewModel.UserRoleId! },
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await httpClient.PostAsJsonAsync("GatewayRole/ReplaceRoleOfUser", apiReplaceRoleOfUserModel);
        }

        //if status code is 201
        TempData["UserCreationSuccess"] = true;
        return RedirectToAction("ManageUsers", "UserManagement");
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string userId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync($"GatewayAdmin/GetUserById/{userId}");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        if ((int)response.StatusCode == 404)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("ManageUsers", "UserManagement");
        }
        else if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        EditUserAccountBasicSettingsViewModel editUserAccountBasicSettingsViewModel = new()
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            AccountActivated = user.EmailConfirmed
        };

        EditUserEmailAccountViewModel editUserEmailAccountViewModel = new()
        {
            UserId = user.Id,
            OldEmail = user.Email
        };

        EditUserPasswordViewModel editUserPasswordViewModel = new()
        {
            UserId = user.Id,
        };

        EditUserViewModel editAccountModel = new()
        {
            EditUserAccountBasicSettingsViewModel = editUserAccountBasicSettingsViewModel,
            EditUserEmailAccountViewModel = editUserEmailAccountViewModel,
            EditUserPasswordViewModel = editUserPasswordViewModel,
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayRole");
        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        //this means the user can't access roles, but they could access users
        if ((int)response.StatusCode >= 403)
        {
            ViewData["UserRoles"] = null;
            return View(editAccountModel);
        }
        responseBody = await response.Content.ReadAsStringAsync();
        List<UiRole> roles = JsonSerializer.Deserialize<List<UiRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        EditUserRoleViewModel editUserRoleViewModel = new()
        {
            UserId = user.Id,
            CurrentRoleId = roles.Where(role => role.Name == user.UserRoleName).FirstOrDefault()?.Id
        };

        //crazy edge case, where the role has been edited/deleted and no longer exists in between requests.
        //In this case just return back to ManageUsers. This will probably never happen
        if (editUserRoleViewModel.CurrentRoleId is null)
            return RedirectToAction("ManageUsers", "UserManagement");

        editAccountModel.EditUserRoleViewModel = editUserRoleViewModel;

        ViewData["UserRoles"] = roles;
        return View(editAccountModel);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeUserBasicSettings(EditUserAccountBasicSettingsViewModel editUserAccountBasicSettingsViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("EditUser", "UserManagement");
        }

        if (!ModelState.IsValid)
        {
            ViewData["userId"] = editUserAccountBasicSettingsViewModel.UserId;
            return View("EditUser");
        }

        UiUser updatedUser = new()
        {
            Id = editUserAccountBasicSettingsViewModel.UserId,
            FirstName = editUserAccountBasicSettingsViewModel.FirstName,
            LastName = editUserAccountBasicSettingsViewModel.LastName,
            PhoneNumber = editUserAccountBasicSettingsViewModel.PhoneNumber
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayAdmin", new { AppUser = updatedUser, ActivateEmail = editUserAccountBasicSettingsViewModel.AccountActivated });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditUser", "UserManagement", new { userId = editUserAccountBasicSettingsViewModel.UserId });
        if (validationResult is not null)
            return validationResult;

        TempData["AccountBasicSettingsChangeSuccess"] = true;
        return RedirectToAction("EditUser", "UserManagement", new { userId = editUserAccountBasicSettingsViewModel.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeUserPassword(EditUserPasswordViewModel editUserPasswordViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewData["userId"] = editUserPasswordViewModel.UserId;
            return View("EditUser");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayAdmin", new { AppUser = new UiUser() { Id = editUserPasswordViewModel.UserId }, Password = editUserPasswordViewModel.NewPassword });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditUser", "UserManagement", new { userId = editUserPasswordViewModel.UserId });
        if (validationResult is not null)
            return validationResult;

        TempData["PasswordChangeSuccess"] = true;
        return RedirectToAction("EditUser", "UserManagement", new { userId = editUserPasswordViewModel.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeUserEmailAccount(EditUserEmailAccountViewModel editUserEmailAccountViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewData["userId"] = editUserEmailAccountViewModel.UserId;
            return View("EditUser");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayAdmin", new { AppUser = new UiUser() { Id = editUserEmailAccountViewModel.UserId, Email = editUserEmailAccountViewModel.NewEmail } });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditUser", "UserManagement", new { userId = editUserEmailAccountViewModel.UserId });
        if (validationResult is not null)
            return validationResult;

        //log out the user
        TempData["EmailChangeSuccess"] = true;
        return RedirectToAction("EditUser", "UserManagement", new { userId = editUserEmailAccountViewModel.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeUserRole(EditUserRoleViewModel editUserRoleViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewData["userId"] = editUserRoleViewModel.UserId;
            return View("EditUser");
        }

        var apiReplaceRoleOfUserModel = new Dictionary<string, string>
        {
            { "userId", editUserRoleViewModel.UserId! },
            { "currentRoleId", editUserRoleViewModel.CurrentRoleId! },
            { "newRoleId", editUserRoleViewModel.NewRoleId! },
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayRole/ReplaceRoleOfUser", apiReplaceRoleOfUserModel);
        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditUser", "UserManagement", new { userId = editUserRoleViewModel.UserId });
        if (validationResult is not null)
            return validationResult;

        //log out the user
        TempData["RoleChangeSuccess"] = true;
        return RedirectToAction("EditUser", "UserManagement", new { userId = editUserRoleViewModel.UserId });
    }
}
