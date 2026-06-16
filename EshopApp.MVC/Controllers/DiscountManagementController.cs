using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.ViewModels.DiscountModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class DiscountManagementController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<RoleManagementController> _logger;

    public DiscountManagementController(IHttpClientFactory httpClientFactory, ILogger<RoleManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageDiscounts()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await httpClient.GetAsync("GatewayAuthentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/ClaimType/Permission/ClaimValue/CanManageProducts");
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            return RedirectToAction("Forbidden403", "Error");
        else if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseDiscount = await httpClient.GetAsync("GatewayDiscount/Amount/10000/includeDeactivated/true");
        var responseProduct = await httpClient.GetAsync("GatewayVariant/Amount/10000/includeDeactivated/true");

        if (responseDiscount.StatusCode == HttpStatusCode.InternalServerError || responseProduct.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (responseDiscount.StatusCode == HttpStatusCode.ServiceUnavailable || responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)responseDiscount.StatusCode >= 500 || (int)responseProduct.StatusCode >= 500)
            return View("Error");

        var responseBody = await responseDiscount.Content.ReadAsStringAsync();
        List<UiDiscount> discounts = JsonSerializer.Deserialize<List<UiDiscount>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseProduct.Content.ReadAsStringAsync();
        List<UiVariant> variants = JsonSerializer.Deserialize<List<UiVariant>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        List<UpdateDiscountViewModel> updateDiscountViewModels = new List<UpdateDiscountViewModel>();
        foreach (var discount in discounts)
        {
            updateDiscountViewModels.Add(new UpdateDiscountViewModel()
            {
                Id = discount.Id,
                Name = discount.Name,
                IsActivated = discount.IsDeactivated.HasValue ? !discount.IsDeactivated.Value : false,
                Percentage = discount.Percentage.HasValue ? discount.Percentage.Value : 0,
                VariantIds = discount.Variants?.Select(variant => variant.Id).ToList()!
            });
        }

        ManageDiscountsViewModel manageDiscountsViewModel = new ManageDiscountsViewModel()
        {
            Discounts = discounts,
            Variants = variants,
            CreateDiscountViewModel = new CreateDiscountViewModel(),
            UpdateDiscountViewModels = updateDiscountViewModels
        };

        return View(manageDiscountsViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDiscount(string discountId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

        if (!ModelState.IsValid)
            return View("ManageDiscounts");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"{discountId}/includeDeactivated/false");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageDiscounts", "DiscountManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["DiscountDeletionSuccess"] = true;
        return RedirectToAction("ManageDiscounts", "DiscountManagement");
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscount(CreateDiscountViewModel createDiscountViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

        createDiscountViewModel.IsDeactivated = !createDiscountViewModel.IsActivated;

        if (createDiscountViewModel.VariantIds is not null && createDiscountViewModel.VariantIds[0] is not null)
            createDiscountViewModel.VariantIds = createDiscountViewModel.VariantIds[0].Split(',').ToList();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayDiscount", createDiscountViewModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageDiscounts", "DiscountManagement");
        if (validationResult is not null)
        {
            TempData["ErrorFromCreateDiscount"] = true;
            return validationResult;
        }

        //if status code is 201
        TempData["DiscountCreationSuccess"] = true;
        return RedirectToAction("ManageDiscounts", "DiscountManagement");
    }

    [HttpPost]
    public async Task<IActionResult> EditDiscount(UpdateDiscountViewModel updateDiscountViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return RedirectToAction("Unauthorized401", "Error");

        updateDiscountViewModel.IsDeactivated = !updateDiscountViewModel.IsActivated;

        if (updateDiscountViewModel.VariantIds is not null && updateDiscountViewModel.VariantIds[0] is not null)
            updateDiscountViewModel.VariantIds = updateDiscountViewModel.VariantIds[0].Split(',').ToList();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayDiscount", updateDiscountViewModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageDiscounts", "DiscountManagement");
        if (validationResult is not null)
            return validationResult;

        if (response.StatusCode == HttpStatusCode.OK)
            TempData["DiscountDeletionSuccessWarning"] = true; //this means that the variant has been deactivated
        else
            TempData["DiscountDeletionSuccess"] = true; //this means status code = 204, which means that the variant has been fully deleted
        return RedirectToAction("ManageDiscounts", "DiscountManagement");
    }

}
