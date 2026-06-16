using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.AuthModels;
using EshopApp.MVC.ViewModels.CartViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class CartController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<AccountController> _logger;

    public CartController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    //this action is used with AJAX from the front end
    [HttpGet]
    public async Task<IActionResult> InitializeUserCart()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return Json(new { authenticated = false, hadAccessToken = false });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        else if ((int)response.StatusCode == 401)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return Json(new { authenticated = false, hadAccessToken = true });
        }
        else if ((int)response.StatusCode >= 400)
            return Json(new { authenticated = true, hadAccessToken = true });

        var responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        foreach (var cartItem in user.Cart!.CartItems)
        {
            if (cartItem.Variant?.VariantImages is null)
                continue;

            cartItem.Variant.VariantImages = cartItem.Variant.VariantImages.OrderByDescending(variantImage => variantImage.IsThumbNail).ToList();
        }

        return Json(new { success = true, cart = user.Cart });
    }

    [HttpPost]
    public async Task<IActionResult> AddItemToCart([FromBody] AddItemToCartViewModel addItemToCartViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return Json(new { authenticated = false });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");
        var responseBody = await response.Content.ReadAsStringAsync();

        //this deals with 5xx errors
        if ((int)response.StatusCode >= 500)
            return new ObjectResult(new { authenticated = false, errorMessage = "ServerError" }) { StatusCode = (int)response.StatusCode };
        else if ((int)response.StatusCode == 401)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return new ObjectResult(new { authenticated = false, errorMessage = "UserNotAuthenticated" })
            {
                StatusCode = (int)response.StatusCode
            };
        }
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            return new ObjectResult(new { authenticated = false, errorMessage = errorMessage })
            {
                StatusCode = (int)response.StatusCode
            };
        }

        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (user.Cart is null) //some users dont have carts
            return Json(new { authenticated = true });

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        addItemToCartViewModel.CartId = user.Cart.Id;
        response = await httpClient.PostAsJsonAsync("GatewayCart/CartItem", addItemToCartViewModel);
        responseBody = await response.Content.ReadAsStringAsync();

        //this deals with 5xx errors
        if ((int)response.StatusCode >= 500)
            return new ObjectResult(new { authenticated = true, errorMessage = "ServerError" }) { StatusCode = (int)response.StatusCode };
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            return new ObjectResult(new { authenticated = true, errorMessage = errorMessage })
            {
                StatusCode = (int)response.StatusCode
            };
        }

        return Json(new { authenticated = true });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemViewModel updateCartItemViewModel)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return Json(new { authenticated = false });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");
        var responseBody = await response.Content.ReadAsStringAsync();

        //this deals with 5xx errors
        if ((int)response.StatusCode >= 500)
            return new ObjectResult(new { authenticated = false, errorMessage = "ServerError" }) { StatusCode = (int)response.StatusCode };
        else if ((int)response.StatusCode == 401)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return new ObjectResult(new { authenticated = false, errorMessage = "UserNotAuthenticated" })
            {
                StatusCode = (int)response.StatusCode
            };
        }
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            return new ObjectResult(new { authenticated = false, errorMessage = errorMessage })
            {
                StatusCode = (int)response.StatusCode
            };
        }

        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (user.Cart is null) //some users dont have carts
            return Json(new { authenticated = true });

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        response = await httpClient.PutAsJsonAsync("GatewayCart/CartItem", updateCartItemViewModel);
        responseBody = await response.Content.ReadAsStringAsync();

        //this deals with 5xx errors
        if ((int)response.StatusCode >= 500)
            return new ObjectResult(new { authenticated = true, errorMessage = "ServerError" }) { StatusCode = (int)response.StatusCode };
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            return new ObjectResult(new { authenticated = true, errorMessage = errorMessage })
            {
                StatusCode = (int)response.StatusCode
            };
        }

        return Json(new { authenticated = true });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItemFromCart([FromBody] string cartItemId)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return Ok(new { authenticated = false });
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync($"GatewayCart/CartItem/{cartItemId}");
        var responseBody = await response.Content.ReadAsStringAsync();

        //this deals with 5xx errors
        if ((int)response.StatusCode >= 500)
            return new ObjectResult(new { authenticated = false, errorMessage = "ServerError" }) { StatusCode = (int)response.StatusCode };
        else if ((int)response.StatusCode == 401)
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return new ObjectResult(new { authenticated = false, errorMessage = "UserNotAuthenticated" })
            {
                StatusCode = (int)response.StatusCode
            };
        }
        else if ((int)response.StatusCode >= 400)
        {
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            responseObject!.TryGetValue("errorMessage", out string? errorMessage);
            return new ObjectResult(new { authenticated = false, errorMessage = errorMessage })
            {
                StatusCode = (int)response.StatusCode
            };
        }

        return NoContent();
    }


    //this is probably accessed only with AJAX
    [HttpPost]
    public async Task<IActionResult> MergeLocalCart([FromBody] List<AddItemToCartViewModel> addItemToCartViewModels)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (string.IsNullOrEmpty(accessToken))
            return Json(new { result = "userNotAuthenticated" });

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return View("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return View("Error503");
        else if ((int)response.StatusCode >= 500)
            return View("Error");

        var responseBody = await response.Content.ReadAsStringAsync();
        UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (user.Cart is null) //there are some users that dont have a cart
            return Json(new { result = "userCartNotFound" });

        //Now synchronize the items to the cart
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        foreach (AddItemToCartViewModel addItemToCartViewModel in addItemToCartViewModels)
            addItemToCartViewModel.CartId = user.Cart!.Id;
        response = await httpClient.PostAsJsonAsync("GatewayCart/CartItems/Bulk", addItemToCartViewModels);

        //If any error happens in adding the cart items just send the user to index and add a message that something has went wrong in synchronizing the cart
        if ((int)response.StatusCode >= 400)
            return Json(new { result = "cartNotSynchronized" });

        return Json(new { result = "noError" });
    }
}
