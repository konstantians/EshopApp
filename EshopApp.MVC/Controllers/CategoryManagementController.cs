using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.ViewModels.CategoryModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class CategoryManagementController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<RoleManagementController> _logger;

    public CategoryManagementController(IHttpClientFactory httpClientFactory, ILogger<RoleManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageCategories()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await httpClient.GetAsync("GatewayAuthentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/ClaimType/Permission/ClaimValue/CanManageProducts");
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

        var responseCategory = await httpClient.GetAsync("GatewayCategory/Amount/10000");
        var responseProduct = await httpClient.GetAsync("GatewayProduct/Amount/10000/includeDeactivated/true");

        if (responseCategory.StatusCode == HttpStatusCode.InternalServerError || responseProduct.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (responseCategory.StatusCode == HttpStatusCode.ServiceUnavailable || responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)responseCategory.StatusCode >= 500 || (int)responseProduct.StatusCode >= 500)
            return View("Error");

        if ((int)responseCategory.StatusCode >= 400 || (int)responseProduct.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseBody = await responseCategory.Content.ReadAsStringAsync();
        List<UiCategory> categories = JsonSerializer.Deserialize<List<UiCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseProduct.Content.ReadAsStringAsync();
        List<UiProduct> products = JsonSerializer.Deserialize<List<UiProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        List<UpdateCategoryViewModel> updateCategoryViewModels = new List<UpdateCategoryViewModel>();
        foreach (var category in categories)
        {
            updateCategoryViewModels.Add(new UpdateCategoryViewModel() { Id = category.Id, Name = category.Name, ProductIds = category.Products?.Select(product => product.Id).ToList()! });
        }

        ManageCategoryViewModel manageCategoryViewModel = new ManageCategoryViewModel()
        {
            Categories = categories,
            Products = products,
            CreateCategoryViewModel = new CreateCategoryViewModel(),
            UpdateCategoryViewModel = updateCategoryViewModels
        };

        return View(manageCategoryViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(string categoryId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("ManageCategories");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayCategory/{categoryId}");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageCategories", "CategoryManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["CategoryDeletionSuccess"] = true;
        return RedirectToAction("ManageCategories", "CategoryManagement");
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryViewModel createCategoryViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (createCategoryViewModel.ProductIds is not null && createCategoryViewModel.ProductIds[0] is not null)
            createCategoryViewModel.ProductIds = createCategoryViewModel.ProductIds[0].Split(',').ToList();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayCategory", createCategoryViewModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageCategories", "CategoryManagement");
        if (validationResult is not null)
        {
            TempData["ErrorFromCreateCategory"] = true;
            return validationResult;
        }

        //if status code is 201
        TempData["CategoryCreationSuccess"] = true;
        return RedirectToAction("ManageCategories", "CategoryManagement");
    }

    [HttpPost]
    public async Task<IActionResult> EditCategory(UpdateCategoryViewModel updateCategoryViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (updateCategoryViewModel.ProductIds is not null && updateCategoryViewModel.ProductIds[0] is not null)
            updateCategoryViewModel.ProductIds = updateCategoryViewModel.ProductIds[0].Split(',').ToList();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync($"GatewayCategory", updateCategoryViewModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageCategories", "CategoryManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 204
        TempData["CategoryUpdateSuccess"] = true;
        return RedirectToAction("ManageCategories", "CategoryManagement");
    }
}
