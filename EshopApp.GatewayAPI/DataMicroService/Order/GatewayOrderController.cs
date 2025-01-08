using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.DataMicroService.Order.Models.RequestModels;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.DataMicroService.Order;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayOrderController : ControllerBase
{
    //the create order functionality is part of createcheckoutsession and thus there is no endpoint here(that data microservice endpoint is used there)

    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayOrderController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetOrders(int amount)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //get the orders
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Order/Amount/{amount}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayOrder>? orders = JsonSerializer.Deserialize<List<GatewayOrder>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(orders);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //get the order
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Order/{id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayOrder? order = JsonSerializer.Deserialize<GatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(order);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    //This endpoint should only be used if the user picks the option pay by cash
    [HttpPost]
    public async Task<IActionResult> CreateOrderWithoutCheckOutSession(GatewayCreateOrderRequestModel gatewayCreateOrderRequestModel)
    {
        try
        {
            if (gatewayCreateOrderRequestModel.UserCouponId is not null &&
                !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient, emailHttpClient, authHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (gatewayCreateOrderRequestModel.UserCouponId is null &&
                !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient, emailHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //set dataHttpClient
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);

            int couponPercentageDiscount = 0;
            //If Coupon is provided check that it is valid and it belongs to the user
            //also fill the couponPercentageDiscount
            if (!string.IsNullOrEmpty(gatewayCreateOrderRequestModel.UserCouponId))
            {
                //check that an access token has been supplied, this check is made to avoid unnecessary requests
                if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                    !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvidedWhileCouponCodeWasProvided" });

                //request to get the user
                _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
                HttpResponseMessage authenticationResponse = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken")); //this contains retry logic

                if ((int)authenticationResponse.StatusCode >= 400)
                    return await _utilityMethods.CommonHandlingForErrorCodesAsync(authenticationResponse);

                string? authenticationResponseBody = await authenticationResponse.Content.ReadAsStringAsync();
                GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(authenticationResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                authenticationResponse = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(
                    () => dataHttpClient.GetAsync($"Coupon/userId/{appUser!.Id}/includeDeactivated/{true}")); //this contains retry logic

                if ((int)authenticationResponse.StatusCode >= 400)
                    return await _utilityMethods.CommonHandlingForErrorCodesAsync(authenticationResponse);

                authenticationResponseBody = await authenticationResponse.Content.ReadAsStringAsync();
                appUser!.UserCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(authenticationResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                GatewayUserCoupon? gatewayUserCoupon = appUser!.UserCoupons.FirstOrDefault(userCoupon =>
                    userCoupon.Id == gatewayCreateOrderRequestModel.UserCouponId);
                if (gatewayUserCoupon is null)
                    return StatusCode(403, new { ErrorMessage = "UserAccountDoesNotContainThisCoupon" });

                couponPercentageDiscount = gatewayUserCoupon.Coupon!.DiscountPercentage!.Value;
                gatewayCreateOrderRequestModel.UserCouponId = gatewayUserCoupon.Id;
            }

            //create the order
            gatewayCreateOrderRequestModel.IsFinal = true;
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                dataHttpClient.PostAsJsonAsync("Order", gatewayCreateOrderRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string responseBody = await response.Content.ReadAsStringAsync();
            GatewayOrder order = JsonSerializer.Deserialize<GatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            var apiSendEmailModel = new Dictionary<string, string>
                {
                    { "receiver", order!.OrderAddress!.Email! },
                    { "title", "Order Placed Successfully" },
                    { "message", "Here are the details of your order: " } //TODO do this well
                };
            _ = Task.Run(async () =>
            {
                _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrder(GatewayUpdateOrderRequestModel gatewayUpdateOrderRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //update the order
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order", gatewayUpdateOrderRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("UpdateOrderStatus")]
    public async Task<IActionResult> UpdateOrderStatus(GatewayUpdateOrderStatusRequestModel gatewayUpdateOrderStatusRequestModels)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //update the order status
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order/UpdateOrderStatus", gatewayUpdateOrderStatusRequestModels));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //delete the order
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.DeleteAsync($"Order/{id}"));

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
