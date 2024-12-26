using EshopApp.GatewayAPI.AuthMicroService.GatewayAdmin.Models.RequestModels;
using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAdmin;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayAdminController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly IUtilityMethods _utilityMethods;
    private readonly IConfiguration _configuration;

    public GatewayAdminController(IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods, IConfiguration configuration)
    {
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
        _utilityMethods = utilityMethods;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the users
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync("Admin");

            //validate that getting the users has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Admin");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayAppUser>? appUsers = JsonSerializer.Deserialize<List<GatewayAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(appUsers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUserById/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        //eventually get user coupons and cart
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Admin/GetUserById/{userId}");

            //validate that getting the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Admin/GetUserById/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //get the user coupons
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.GetAsync($"Coupon/userId/{userId}/includeDeactivated/true");

            //validate that getting the user coupons has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Coupon/userId/{userId}/includeDeactivated/true");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayUserCoupon>? userCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            appUser!.UserCoupons = userCoupons!;

            return Ok(appUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUserByEmail/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        //eventually get user coupons and cart
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Admin/GetUserByEmail/{email}");

            //validate that getting the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Admin/GetUserByEmail/{email}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //get the user coupons
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.GetAsync($"Coupon/userId/{appUser!.Id}/includeDeactivated/true");

            //validate that getting the user coupons has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Coupon/userId/{appUser.Id}/includeDeactivated/true");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayUserCoupon>? userCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            appUser!.UserCoupons = userCoupons!;

            return Ok(appUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAccount([FromBody] GatewayCreateUserRequestModel gatewayApiCreateUserRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //start by doing healthchecks for the endpoints this is calling
            if (gatewayApiCreateUserRequestModel.SendEmailNotification && !await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            //sendEmailNotification == false check is made to avoid an uneccesary http call
            else if (!gatewayApiCreateUserRequestModel.SendEmailNotification && !await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //send request to create the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Admin",
                new { gatewayApiCreateUserRequestModel.Email, gatewayApiCreateUserRequestModel.Password, gatewayApiCreateUserRequestModel.PhoneNumber });

            //validate that creating the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Admin",
                    new { gatewayApiCreateUserRequestModel.Email, gatewayApiCreateUserRequestModel.Password, gatewayApiCreateUserRequestModel.PhoneNumber });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //send request to create the user cart
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("Cart", new { UserId = appUser!.Id });

            //validate that creating the user cart has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PostAsJsonAsync("Cart", new { UserId = appUser!.Id });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //string? responseBody = await response.Content.ReadAsStringAsync();
            //GatewayCart? cart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //here just connect the user with the cart
            //appUser.Cart = cart;

            //send an email to the user to notify them that their account has been deleted
            if (gatewayApiCreateUserRequestModel.SendEmailNotification)
            {
                var apiSendEmailModel = new Dictionary<string, string>
                {
                   { "receiver", gatewayApiCreateUserRequestModel.Email! },
                    { "title", "Account Deletion" },
                    { "message", "An administrator has deleted your account. If you have any questions you can contact us at kinnaskonstantinos0@gmail.com ." }
                };
                _ = Task.Run(async () =>
                {
                    _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                    await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
                });
            }

            return CreatedAtAction(nameof(GetUserById), new { userId = appUser.Id }, appUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserAccount([FromBody] GatewayUpdateUserRequestModel gatewayApiUpdateUserRequestModel)
    {
        //maybe sent in app notification for update user account. Think about it
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //send request to update the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PutAsJsonAsync("Admin",
                new { gatewayApiUpdateUserRequestModel.AppUser, gatewayApiUpdateUserRequestModel.Password, PhoneNumber = gatewayApiUpdateUserRequestModel.ActivateEmail });

            //validate that updating the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PutAsJsonAsync("Admin",
                    new { gatewayApiUpdateUserRequestModel.AppUser, gatewayApiUpdateUserRequestModel.Password, PhoneNumber = gatewayApiUpdateUserRequestModel.ActivateEmail });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{userId}")]
    [HttpDelete("{userId}/userEmail/{userEmail}")]
    public async Task<IActionResult> DeleteUserAccount(string userId, string? userEmail)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //start by doing healthchecks for the endpoints this is calling
            if (userEmail is not null && !await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (userEmail is null && !await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient })) //userEmail is null check is made to avoid an uneccesary http call
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //send request to delete the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.DeleteAsync($"Admin/{userId}");

            //validate that deleting the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.DeleteAsync($"Admin/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //send request to delete the user cart. Important note: Do not set the default headers for client twice for the same httpclient
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.DeleteAsync($"Cart/UserId/{userId}");

            //validate that deleting the user cart has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"Cart/UserId/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //send request to delete the coupons of the user
            response = await dataHttpClient.DeleteAsync($"Coupon/RemoveAllUserCoupons/userId/{userId}");

            //validate that deleting the coupons of the user has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"Coupon/RemoveAllUserCoupons/userId/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //send an email to the user to notify them that their account has been deleted
            if (userEmail is not null)
            {
                var apiSendEmailModel = new Dictionary<string, string>
                {
                   { "receiver", userEmail },
                    { "title", "Account Deletion" },
                    { "message", "An administrator has deleted your account. If you have any questions you can contact us at kinnaskonstantinos0@gmail.com ." }
                };
                _ = Task.Run(async () =>
                {
                    _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                    await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
                });
            }

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
