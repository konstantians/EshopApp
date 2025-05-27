using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class RoleManagementController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<RoleManagementController> _logger;

    public RoleManagementController(IHttpClientFactory httpClientFactory, ILogger<RoleManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageRoles()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayRole");

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
        List<UiRole> roles = JsonSerializer.Deserialize<List<UiRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayRole/GetClaims");
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

        responseBody = await response.Content.ReadAsStringAsync();
        List<UiClaim> uiClaims = JsonSerializer.Deserialize<List<UiClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        ViewData["SystemClaims"] = uiClaims;
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("ManageRoles");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayRole/{roleId}");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageRoles", "RoleManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["RoleDeletionSuccess"] = true;
        return RedirectToAction("ManageRoles", "RoleManagement");
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(string roleName, string[] selectedClaims)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        List<UiClaim> selectedUiClaims = new List<UiClaim>();
        foreach (string selectedClaim in selectedClaims)
        {
            string[] claimParts = selectedClaim.Split("|");
            selectedUiClaims.Add(new UiClaim() { Type = claimParts[0], Value = claimParts[1] });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayRole", new { RoleName = roleName, Claims = selectedUiClaims });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "CreateUser", "RoleManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 201
        TempData["RoleCreationSuccess"] = true;
        return RedirectToAction("ManageRoles", "RoleManagement");
    }

    [HttpPost]
    public async Task<IActionResult> EditRole(string roleId, string[] newClaims)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        List<UiClaim> newUiClaims = new List<UiClaim>();
        foreach (string selectedClaim in newClaims)
        {
            string[] claimParts = selectedClaim.Split("|");
            newUiClaims.Add(new UiClaim() { Type = claimParts[0], Value = claimParts[1] });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync($"GatewayRole/UpdateClaimsOfRole", new { RoleId = roleId, NewClaims = newUiClaims });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageRoles", "RoleManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["RoleUpdateSuccess"] = true;
        return RedirectToAction("ManageRoles", "RoleManagement");
    }
}
