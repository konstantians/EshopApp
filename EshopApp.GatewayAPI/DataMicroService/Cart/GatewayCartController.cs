using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.DataMicroService.Cart.Models.RequestModels;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Text.Json;

namespace EshopApp.GatewayAPI.DataMicroService.Cart;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayCartController : ControllerBase
{
    //the other cart actions will not be used by the app that uses the gateway api for security reasons and because they are encompassed in the
    //SingUp, GetUserByAccessToken, DeleteUser, CreateUser endpoints of the gateway api. The endpoints that are left are needed for cart updates.
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayCartController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpPost("CartItem")]
    public async Task<IActionResult> CreateCartItem(GatewayCreateCartItemRequestModel gatewayCreateCartItemRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //request to get the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //get the cart of the user
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Cart/UserId/{appUser!.Id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            GatewayCart? userCart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //this checks that the given cartId belongs to the cart of the user
            if (userCart!.Id != gatewayCreateCartItemRequestModel.CartId)
                return StatusCode(403, new { ErrorMessage = "GivenCartDoesNotBelongToGivenUser" });

            //create the cartItem
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PostAsJsonAsync("Cart/CartItem", gatewayCreateCartItemRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            GatewayCartItem? cartItem = JsonSerializer.Deserialize<GatewayCartItem>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.StatusCode == HttpStatusCode.OK) //this happens if the cartItem already existed and its quantity just adjusted
                return Ok(cartItem);

            return Created("", cartItem);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("CartItem")]
    public async Task<IActionResult> UpdateCartItem(GatewayUpdateCartItemRequestModel gatewayUpdateCartItemRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //request to get the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //get the cart of the user
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Cart/UserId/{appUser!.Id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            GatewayCart? userCart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //this checks whether or not the user has the given cart item
            if (!userCart!.CartItems.Any(cartItem => cartItem.Id == gatewayUpdateCartItemRequestModel.CartItemId))
                return StatusCode(403, new { ErrorMessage = "GivenCartItemDoesNotBelongToGivenUser" });

            //create the cartItem
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Cart/CartItem", gatewayUpdateCartItemRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("CartItem/{id}")]
    public async Task<IActionResult> DeleteCartItem(string id)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //request to get the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //get the cart of the user
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Cart/UserId/{appUser!.Id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            GatewayCart? userCart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!userCart!.CartItems.Any(cartItem => cartItem.Id == id))
                return StatusCode(403, new { ErrorMessage = "GivenCartItemDoesNotBelongToGivenUser" });

            //create the cartItem
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.DeleteAsync($"Cart/CartItem/{id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
