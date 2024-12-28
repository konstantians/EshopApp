using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.DataMicroService.ShippingOption.Models;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Text.Json;

namespace EshopApp.GatewayAPI.DataMicroService.ShippingOption;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayShippingOptionController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayShippingOptionController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetShippingOptions(int amount, bool includeDeactivated)
    {
        try
        {
            //get the shippingOptions
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"ShippingOption/Amount/{amount}/includeDeactivated/{includeDeactivated}");

            //validate that getting the shippingOptions has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"ShippingOption/Amount/{amount}/includeDeactivated/{includeDeactivated}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayShippingOption>? shippingOptions = JsonSerializer.Deserialize<List<GatewayShippingOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(shippingOptions);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetShippingOptionById(string id, bool includeDeactivated)
    {
        try
        {
            //get the shippingOption
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"ShippingOption/{id}/includeDeactivated/{includeDeactivated}");

            //validate that getting the shippingOption has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"ShippingOption/{id}/includeDeactivated/{includeDeactivated}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayShippingOption? shippingOption = JsonSerializer.Deserialize<GatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(shippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateShippingOption(GatewayCreateShippingOptionRequestModel gatewayCreateShippingOptionRequestModel)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //create the shippingOption
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("ShippingOption", gatewayCreateShippingOptionRequestModel);

            //validate that creating the shippingOption has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PostAsJsonAsync("ShippingOption", gatewayCreateShippingOptionRequestModel);
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayShippingOption? shippingOption = JsonSerializer.Deserialize<GatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return CreatedAtAction(nameof(GetShippingOptionById), new { id = shippingOption!.Id, includeDeactivated = true }, shippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateShippingOption(GatewayUpdateShippingOptionRequestModel gatewayUpdateShippingOptionRequestModel)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //update the shipping option
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PutAsJsonAsync("ShippingOption", gatewayUpdateShippingOptionRequestModel);

            //validate that updating the shipping option has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PutAsJsonAsync("ShippingOption", gatewayUpdateShippingOptionRequestModel);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShippingOption(string id)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrderOptions");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //delete the shipping option
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.DeleteAsync($"ShippingOption/{id}");

            //validate that deleting the shipping option has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"ShippingOption/{id}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            if (response.StatusCode == HttpStatusCode.OK) //this can happen if the shipping option exists in an order and thus it only becomes deactivated/soft deleted
                return Ok(new { WarningMessage = "NoErrorButNotFullyDeleted" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
