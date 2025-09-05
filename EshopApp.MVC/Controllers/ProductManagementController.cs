using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.ViewModels.CreateProductModels;
using EshopApp.MVC.ViewModels.EditProductModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class ProductManagementController : Controller
{
    private readonly ILogger<ProductManagementController> _logger;
    private readonly HttpClient httpClient;

    public ProductManagementController(IHttpClientFactory httpClientFactory, ILogger<ProductManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageProducts()
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

        var responseProduct = await httpClient.GetAsync("GatewayProduct/Amount/10000/includeDeactivated/true");

        if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)responseProduct.StatusCode >= 500)
            return View("Error");

        if ((int)responseProduct.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        var responseBody = await responseProduct.Content.ReadAsStringAsync();
        List<UiProduct> products = JsonSerializer.Deserialize<List<UiProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
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

        HttpResponseMessage responseCategory = await httpClient.GetAsync("GatewayCategory/Amount/10000");
        HttpResponseMessage responseDiscounts = await httpClient.GetAsync("GatewayDiscount/Amount/10000/includeDeactivated/false");
        HttpResponseMessage responseImages = await httpClient.GetAsync("GatewayImage/Amount/10000/includeSoftDeleted/false");

        //this deals with 5xx errors
        if (responseCategory.StatusCode == HttpStatusCode.InternalServerError || responseDiscounts.StatusCode == HttpStatusCode.InternalServerError || responseImages.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (responseCategory.StatusCode == HttpStatusCode.ServiceUnavailable || responseDiscounts.StatusCode == HttpStatusCode.ServiceUnavailable || responseImages.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)responseCategory.StatusCode >= 500 || (int)responseDiscounts.StatusCode > 400 || (int)responseImages.StatusCode > 400)
            return View("Error");

        if ((int)responseCategory.StatusCode >= 400 || (int)responseDiscounts.StatusCode > 400 || (int)responseImages.StatusCode > 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        string responseBody = await responseCategory.Content.ReadAsStringAsync();
        List<UiCategory> categories = JsonSerializer.Deserialize<List<UiCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseDiscounts.Content.ReadAsStringAsync();
        List<UiDiscount> discounts = JsonSerializer.Deserialize<List<UiDiscount>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseImages.Content.ReadAsStringAsync();
        List<UiImage> images = JsonSerializer.Deserialize<List<UiImage>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        ViewData["Categories"] = categories;
        ViewData["Discounts"] = discounts;
        ViewData["Images"] = images;
        return View(new CreateProductViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductViewModel createProductViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("CreateProduct");

        createProductViewModel.CategoryIds = createProductViewModel.CategoryIds ?? new List<string>();
        createProductViewModel.IsDeactivated = !createProductViewModel.IsActivated;
        createProductViewModel.CreateVariantRequestModel!.IsDeactivated = !createProductViewModel.CreateVariantRequestModel.IsActivated;
        createProductViewModel.CreateVariantRequestModel!.AttributeIds = createProductViewModel.CreateVariantRequestModel.AttributeIds ?? new List<string>();
        createProductViewModel.CreateVariantRequestModel!.VariantImageRequestModels = createProductViewModel.CreateVariantRequestModel.VariantImageRequestModels ?? new List<CreateVariantImageViewModel>();
        createProductViewModel.CreateVariantRequestModel!.DiscountId = null;

        if (createProductViewModel.CreateVariantRequestModel.ImageIds is not null)
        {
            string[] imageIds = createProductViewModel.CreateVariantRequestModel.ImageIds.Split(',');
            foreach (var imageId in imageIds)
            {
                createProductViewModel.CreateVariantRequestModel.VariantImageRequestModels.Add(new CreateVariantImageViewModel { ImageId = imageId, IsThumbNail = false });
            }
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayProduct", createProductViewModel);
        var errorContent = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "CreateProduct", "productManagement");
        if (validationResult is not null)
            return validationResult;

        //if status code is 201
        TempData["ProductCreationSuccess"] = true;
        return RedirectToAction("ManageProducts", "ProductManagement");
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(string productId, string? variantId)
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

        HttpResponseMessage responseCategory = await httpClient.GetAsync("GatewayCategory/Amount/10000");
        HttpResponseMessage responseDiscounts = await httpClient.GetAsync("GatewayDiscount/Amount/10000/includeDeactivated/false");
        HttpResponseMessage responseImages = await httpClient.GetAsync("GatewayImage/Amount/10000/includeSoftDeleted/false");
        HttpResponseMessage getProductResponse = await httpClient.GetAsync($"GatewayProduct/{productId}/includeDeactivated/true");

        //this deals with 5xx errors
        if (responseCategory.StatusCode == HttpStatusCode.InternalServerError || responseDiscounts.StatusCode == HttpStatusCode.InternalServerError ||
            responseImages.StatusCode == HttpStatusCode.InternalServerError || getProductResponse.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (responseCategory.StatusCode == HttpStatusCode.ServiceUnavailable || responseDiscounts.StatusCode == HttpStatusCode.ServiceUnavailable ||
            responseImages.StatusCode == HttpStatusCode.ServiceUnavailable || getProductResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)responseCategory.StatusCode >= 500 || (int)responseDiscounts.StatusCode > 400 || (int)responseImages.StatusCode > 400 || (int)getProductResponse.StatusCode > 400)
            return View("Error");

        if ((int)responseCategory.StatusCode >= 400 || (int)responseDiscounts.StatusCode > 400 || (int)responseImages.StatusCode > 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        EditProductContainerViewModel editProductContainerViewModel = new EditProductContainerViewModel();

        string responseBody = await responseCategory.Content.ReadAsStringAsync();
        List<UiCategory> categories = JsonSerializer.Deserialize<List<UiCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseDiscounts.Content.ReadAsStringAsync();
        List<UiDiscount> discounts = JsonSerializer.Deserialize<List<UiDiscount>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        responseBody = await responseImages.Content.ReadAsStringAsync();
        List<UiImage> images = JsonSerializer.Deserialize<List<UiImage>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        responseBody = await getProductResponse.Content.ReadAsStringAsync();
        UiProduct? product = JsonSerializer.Deserialize<UiProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        List<EditVariantViewModel> editVariantViewModels = new List<EditVariantViewModel>();
        foreach (UiVariant variant in product!.Variants)
        {
            EditVariantViewModel editVariantViewModel = new EditVariantViewModel();
            editVariantViewModel.Id = variant.Id;
            editVariantViewModel.SKU = variant.SKU;
            editVariantViewModel.Price = variant.Price;
            editVariantViewModel.UnitsInStock = variant.UnitsInStock;
            editVariantViewModel.IsThumbnailVariant = variant.IsThumbnailVariant ?? false;
            editVariantViewModel.IsDeactivated = variant.IsDeactivated ?? false;
            editVariantViewModel.IsActivated = !editVariantViewModel.IsDeactivated;
            editVariantViewModel.Discount = variant.Discount;
            editVariantViewModel.DiscountId = variant.DiscountId;
            editVariantViewModel.ImagesIds = variant.VariantImages.Select(variantImage => variantImage.ImageId).ToList()!;
            editVariantViewModel.ImageIdThatShouldBeThumbnail = variant.VariantImages.FirstOrDefault(variantImage => variantImage.IsThumbNail)?.ImageId;
            editVariantViewModel.AttributeIds = variant.Attributes.Select(attribute => attribute.Id).ToList()!;
            editVariantViewModel.ProductId = product!.Id;

            editVariantViewModels.Add(editVariantViewModel);
        }

        EditProductViewModel editProductViewModel = new EditProductViewModel()
        {
            Id = product!.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            IsDeactivated = product.IsDeactivated ?? false,
            IsActivated = !(product.IsDeactivated ?? false),
            CategoryIds = product.Categories.Select(category => category.Id).ToList()!,
        };

        editProductContainerViewModel.EditProductViewModel = editProductViewModel;
        editProductContainerViewModel.EditVariantViewModels = editVariantViewModels;

        ViewData["Categories"] = categories;
        ViewData["Discounts"] = discounts;
        ViewData["Images"] = images;
        ViewData["VariantId"] = variantId;
        return View(editProductContainerViewModel);
    }

    //break the editproductviewmodel to productInformation and the list of variants(that part already exists)
    //here just pass the product information
    [HttpPost]
    public async Task<IActionResult> EditProduct(EditProductViewModel editProductViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditProduct");

        editProductViewModel.IsDeactivated = !editProductViewModel.IsActivated;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayProduct", editProductViewModel);
        var errorContent = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditProduct", "productManagement", new { productId = editProductViewModel.Id });
        if (validationResult is not null)
            return validationResult;

        //if status code is 201
        TempData["ProductUpdateSuccess"] = true;
        return RedirectToAction("EditProduct", "ProductManagement", new { productId = editProductViewModel.Id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(string productId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("ManageProducts");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayProduct/{productId}");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "ManageProducts", "ProductManagement");
        if (validationResult is not null)
            return validationResult;

        if (response.StatusCode == HttpStatusCode.OK)
            TempData["ProductDeletionSuccessWarning"] = true; //this means that the product has been deactivated
        else
            TempData["ProductDeletionSuccess"] = true; //this means status code = 204, which means that the product has been fully deleted

        return RedirectToAction("ManageProducts", "ProductManagement");
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant(CreateVariantViewModel createVariantViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditProduct");

        createVariantViewModel.IsDeactivated = !createVariantViewModel.IsActivated;
        createVariantViewModel!.AttributeIds = createVariantViewModel.AttributeIds ?? new List<string>();
        createVariantViewModel!.VariantImageRequestModels = new List<CreateVariantImageViewModel>();
        createVariantViewModel!.DiscountId = null;

        if (createVariantViewModel.ImageIds is not null)
        {
            string[] imageIds = createVariantViewModel.ImageIds.Split(',');
            foreach (var imageId in imageIds)
            {
                createVariantViewModel.VariantImageRequestModels.Add(new CreateVariantImageViewModel { ImageId = imageId, IsThumbNail = false });
            }
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("GatewayVariant", createVariantViewModel);

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditProduct", "ProductManagement", new { productId = createVariantViewModel.ProductId });
        if (validationResult is not null)
        {
            TempData["ErrorFromCreateVariant"] = true;
            return validationResult;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        UiVariant? createdVariant = JsonSerializer.Deserialize<UiVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        //if status code is 201
        TempData["VariantCreationSuccess"] = true;
        return RedirectToAction("EditProduct", "ProductManagement", new { productId = createVariantViewModel.ProductId, variantId = createdVariant.Id });
    }

    [HttpPost]
    public async Task<IActionResult> EditVariant(EditVariantViewModel editVariantViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("EditProduct");

        editVariantViewModel.IsDeactivated = !editVariantViewModel.IsActivated;
        if (editVariantViewModel.ImagesIds is not null && editVariantViewModel.ImagesIds.Count > 0)
            editVariantViewModel.ImagesIds = editVariantViewModel.ImagesIds[0] is not null ? editVariantViewModel.ImagesIds[0].Split(',').ToList() : null;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PutAsJsonAsync("GatewayVariant", editVariantViewModel);
        var errorContent = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "EditProduct", "productManagement", new { productId = editVariantViewModel.ProductId, variantId = editVariantViewModel.Id });
        if (validationResult is not null)
            return validationResult;

        //if status code is 201
        TempData["VariantUpdateSuccess"] = true;
        return RedirectToAction("EditProduct", "ProductManagement", new { productId = editVariantViewModel.ProductId, variantId = editVariantViewModel.Id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteVariant(string variantId, string? productId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (!ModelState.IsValid)
            return View("ManageProducts");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayVariant/{variantId}");

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, productId is null ? "ManageProducts" : "EditProduct", "ProductManagement",
            routeValues: productId is not null ? new { productId = productId, variantId = variantId } : null);
        if (validationResult is not null)
            return validationResult;

        if (response.StatusCode == HttpStatusCode.OK)
            TempData["VariantDeletionSuccessWarning"] = true; //this means that the variant has been deactivated
        else
            TempData["VariantDeletionSuccess"] = true; //this means status code = 204, which means that the variant has been fully deleted

        if (productId is not null)
            return RedirectToAction("EditProduct", "ProductManagement", new { productId = productId });
        else
            return RedirectToAction("ManageProducts", "ProductManagement");
    }

}
