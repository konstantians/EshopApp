using EshopApp.GatewayAPI.HelperMethods;
using EshopApp.GatewayAPI.Models;
using EshopApp.GatewayAPI.Models.RequestModels.GatewayAdminControllerRequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Controllers;

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(appUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAccount([FromBody] GatewayApiCreateUserRequestModel gatewayApiCreateUserRequestModel)
    {
        try
        {
            //start by doing healthchecks for the endpoints this is calling
            if (gatewayApiCreateUserRequestModel.SendEmailNotification && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            //sendEmailNotification == false check is made to avoid an uneccesary http call
            else if (!gatewayApiCreateUserRequestModel.SendEmailNotification && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //send request to create the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Admin",
                new { Email = gatewayApiCreateUserRequestModel.Email, Password = gatewayApiCreateUserRequestModel.Password, PhoneNumber = gatewayApiCreateUserRequestModel.PhoneNumber });

            //validate that creating the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Admin",
                    new { Email = gatewayApiCreateUserRequestModel.Email, Password = gatewayApiCreateUserRequestModel.Password, PhoneNumber = gatewayApiCreateUserRequestModel.PhoneNumber });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
    public async Task<IActionResult> UpdateUserAccount([FromBody] GatewayApiUpdateUserRequestModel gatewayApiUpdateUserRequestModel)
    {
        //maybe sent in app notification for update user account. Think about it
        try
        {
            //send request to update the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PutAsJsonAsync("Admin",
                new { AppUser = gatewayApiUpdateUserRequestModel.AppUser, Password = gatewayApiUpdateUserRequestModel.Password, PhoneNumber = gatewayApiUpdateUserRequestModel.ActivateEmail });

            //validate that updating the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PutAsJsonAsync("Admin",
                    new { AppUser = gatewayApiUpdateUserRequestModel.AppUser, Password = gatewayApiUpdateUserRequestModel.Password, PhoneNumber = gatewayApiUpdateUserRequestModel.ActivateEmail });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
            //start by doing healthchecks for the endpoints this is calling
            if (userEmail is not null && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (userEmail is null && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient }))) //userEmail is null check is made to avoid an uneccesary http call
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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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

    private async Task<IActionResult> CommonValidationForRequestClientErrorCodesAsync(HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        //in the case there is no body
        if (string.IsNullOrEmpty(responseBody))
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return Unauthorized();
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                return StatusCode(StatusCodes.Status403Forbidden);
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest();
            else if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound();
            else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                return NotFound();

            return BadRequest(); //this will probably never happen
        }

        //otherwise
        var keyValue = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        keyValue!.TryGetValue("errorMessage", out object? errorMessageObject);
        string? errorMessage = errorMessageObject?.ToString() ?? null;
        keyValue!.TryGetValue("errors", out var errors);

        if (response.StatusCode == HttpStatusCode.Unauthorized && errorMessage is not null)
            return Unauthorized(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Unauthorized();
        else if (response.StatusCode == HttpStatusCode.Forbidden && errorMessage is not null)
            return StatusCode(StatusCodes.Status403Forbidden, new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            return StatusCode(StatusCodes.Status403Forbidden);
        else if (response.StatusCode == HttpStatusCode.BadRequest && errorMessage is not null)
            return BadRequest(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.BadRequest && errors is not null) //this is for request validation errors
            return BadRequest(new { Errors = errors });
        else if (response.StatusCode == HttpStatusCode.NotFound && errorMessage is not null)
            return NotFound(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();
        else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            return StatusCode(StatusCodes.Status405MethodNotAllowed);

        return BadRequest(); //this will probably never happen
    }
}
