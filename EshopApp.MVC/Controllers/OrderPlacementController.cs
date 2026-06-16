using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.AuthModels;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.ViewModels.CartViewModels;
using EshopApp.MVC.ViewModels.OrderPlacementModels;
using EshopApp.MVC.ViewModels.SignInAndSignUpModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class OrderPlacementController : Controller
{
    private readonly ILogger<OrderPlacementController> _logger;
    private readonly HttpClient httpClient;

    public OrderPlacementController(IHttpClientFactory httpClientFactory, ILogger<OrderPlacementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> CustomerAccountTypeSelection()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

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
            if (user.Cart!.CartItems.Count == 0 && (TempData["ShouldSynchronizeBackEndCart"] is null || (bool?)TempData["ShouldSynchronizeBackEndCart"] == false))
                return RedirectToAction("ViewCart", "Home");
            else if (TempData["ShouldSynchronizeBackEndCart"] is not null && (bool?)TempData["ShouldSynchronizeBackEndCart"] == true)
                TempData.Keep("ShouldSynchronizeBackEndCart");

            //if eveything is fine then just skip this step, because the user is already logged in
            return RedirectToAction("CustomerInformation", "OrderPlacement");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CustomerAccountTypeSelectionSignIn(SignInViewModel signInViewModel)
    {
        //if there is an access token just send them to homepage
        if (!string.IsNullOrEmpty(Request.Cookies["EshopAppAuthenticationCookie"]))
            return RedirectToAction("CustomerInformation", "OrderPlacement");

        string rawValue = signInViewModel.UserCartJson ?? "[]";
        List<AddItemToCartViewModel> addItemToCartViewModels;
        //For whatever reason the string is not sent correctly, so yes I give up this fixes it
        if (rawValue.StartsWith("\""))
            rawValue = JsonSerializer.Deserialize<string>(rawValue) ?? "[]";

        addItemToCartViewModels = JsonSerializer.Deserialize<List<AddItemToCartViewModel>>(rawValue, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AddItemToCartViewModel>();

        //if somehow the user signs in without an item to their cart they should not be here and should be redirected to home
        if (addItemToCartViewModels is null || !addItemToCartViewModels.Any())
            return RedirectToAction("ViewCart", "Home");

        //if the model state is invalid
        if (!ModelState.IsValid)
            return View("CustomerAccountTypeSelection");

        var apiSignInModel = new Dictionary<string, string>
        {
            { "email", signInViewModel.Email! },
            { "password", signInViewModel.Password! }
        };

        var response = await httpClient.PostAsJsonAsync("GatewayAuthentication/SignIn", apiSignInModel);
        var responseBody = await response.Content.ReadAsStringAsync();

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, responseBody, "CustomerAccountTypeSelection", "OrderPlacement", responseBodyWasPassedIn: true);
        if (validationResult is not null)
            return validationResult;

        //if status code is 200
        Dictionary<string, string>? noErrorResponseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        noErrorResponseObject!.TryGetValue("accessToken", out string? accessToken);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true
        };

        //no value means that the cookie will be destroyed when the browser closes
        if (signInViewModel.RememberMe)
            cookieOptions.Expires = DateTimeOffset.Now.AddDays(30);

        Response.Cookies.Append("EshopAppAuthenticationCookie", accessToken!, cookieOptions);

        //Get users cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");
        //This happens when a user does not have a cart
        else if (response.StatusCode == HttpStatusCode.NotFound)
            return RedirectToAction("Index", "Home");
        else if ((int)response.StatusCode >= 400)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        //Now synchronize the items to the cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        foreach (AddItemToCartViewModel addItemToCartViewModel in addItemToCartViewModels)
            addItemToCartViewModel.CartId = user.Cart!.Id;
        response = await httpClient.PostAsJsonAsync("GatewayCart/CartItems/Bulk", addItemToCartViewModels);

        //If any error happens in adding the cart items just send the user to index and add a message that something has went wrong in synchronizing the cart
        if ((int)response.StatusCode >= 400)
        {
            TempData["CartSynchronizationFailure"] = true;
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("CustomerInformation", "OrderPlacement");
    }

    [HttpGet]
    public async Task<IActionResult> CustomerInformation()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

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
            if (user.Cart!.CartItems.Count == 0 && (TempData["ShouldSynchronizeBackEndCart"] is null || (bool?)TempData["ShouldSynchronizeBackEndCart"] == false)) //this means that external login happened from CustomerAccountTypeSelection
                return RedirectToAction("ViewCart", "Home");

            ViewData["ShouldSynchronizeCart"] = true;
            return View(user);
        }

        return View(null);
    }

    [HttpGet]
    public async Task<IActionResult> OrderInformation()
    {
        HttpResponseMessage response = await httpClient.GetAsync("GatewayShippingOption/amount/15/includeDeactivated/false");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<UiShippingOption>? shippingOptions = JsonSerializer.Deserialize<List<UiShippingOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        shippingOptions = shippingOptions?.OrderBy(shippingOption => shippingOption.ExtraCost).ToList();
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

        response = await httpClient.GetAsync("GatewayPaymentOption/amount/15/includeDeactivated/false");
        responseBody = await response.Content.ReadAsStringAsync();
        List<UiPaymentOption>? paymentOptions = JsonSerializer.Deserialize<List<UiPaymentOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        paymentOptions = paymentOptions?.OrderBy(paymentOption => paymentOption.ExtraCost).ToList();
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

        OrderInformationViewModel orderInformationViewModel = new OrderInformationViewModel();
        orderInformationViewModel.UiPaymentOptions = paymentOptions!;
        orderInformationViewModel.UiShippingOptions = shippingOptions!;

        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

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

            responseBody = await response.Content.ReadAsStringAsync();
            UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (user.Cart!.CartItems.Count == 0)
                return RedirectToAction("ViewCart", "Home");

            orderInformationViewModel.User = user;
            ViewData["ShouldSynchronizeCart"] = true;
            return View(orderInformationViewModel);
        }

        orderInformationViewModel.User = null;
        return View(orderInformationViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckOutSession([FromBody] CreateCheckoutSessionViewModel createCheckoutSessionViewModel)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("GatewayCheckOutSession", createCheckoutSessionViewModel);

            if (response.IsSuccessStatusCode)
                return NoContent();
            else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                return StatusCode((int)response.StatusCode, new { errorMessage = error?["errorMessage"] ?? "ClientError" });
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                return StatusCode(500, new { errorMessage = "ServerError" });
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode(503, new { errorMessage = "ServiceUnavailable" });

            return StatusCode(500, new { errorMessage = "UnknownError" });
        }
        catch
        {
            return StatusCode(500, new { errorMessage = "Exception" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrderWithoutCheckOutSession([FromBody] CreateOrderViewModel createOrderViewModel)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("GatewayOrder", createOrderViewModel);

            if (response.IsSuccessStatusCode)
                return NoContent();
            else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                return StatusCode((int)response.StatusCode, new { errorMessage = error?["errorMessage"] ?? "ClientError" });
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                return StatusCode(500, new { errorMessage = "ServerError" });
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode(503, new { errorMessage = "ServiceUnavailable" });

            return StatusCode(500, new { errorMessage = "UnknownError" });
        }
        catch
        {
            return StatusCode(500, new { errorMessage = "Exception" });
        }
    }

    //TODO There is a potential bug if an admin changes an sku of product if that is in the cart, so maybe do that with Id instead. For now that is good enough
    [HttpPost]
    public async Task<IActionResult> ValidateAllCartItems([FromBody] List<CartItemValidationModel> cartItemValidationModels)
    {
        try
        {
            List<string> skus = new List<string>();
            foreach (CartItemValidationModel cartItemValidationModel in cartItemValidationModels)
                skus.Add(cartItemValidationModel.Sku!);

            var query = string.Join("&", skus.Select(s => $"skus={Uri.EscapeDataString(s)}"));
            var url = $"GatewayVariant/GetVariantsBySkus/includeDeactivated/false?{query}";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<UiVariant>? variants = JsonSerializer.Deserialize<List<UiVariant>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (CartItemValidationModel cartItemValidationModel in cartItemValidationModels)
                    //the 0 can happen only if the sku has changed or the variant was deactivated
                    cartItemValidationModel.UnitsInStockAtCurrentMoment = variants!.FirstOrDefault(variant => variant.SKU == cartItemValidationModel.Sku)?.UnitsInStock ?? 0;

                return Ok(new { validatedVariants = cartItemValidationModels });
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
                return StatusCode(500, new { errorMessage = "ServerError" });
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode(503, new { errorMessage = "ServiceUnavailable" });

            return StatusCode(500, new { errorMessage = "UnknownError" });
        }
        catch
        {
            return StatusCode(500, new { errorMessage = "Exception" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> OrderSuccess()
    {
        //Then on front end delete order stuff
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return View();

        //if the user is authenticated
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

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

        string responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (user.Cart!.CartItems.Count == 0)
            return RedirectToAction("ViewCart", "Home");

        //Here delete cart
        response = await httpClient.DeleteAsync($"GatewayCart/CartItems/{user.Cart.Id}");
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

        ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }

    [HttpGet]
    public IActionResult OrderFailure()
    {
        //Then on front end delete order stuff
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
            return View();

        ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }
}
