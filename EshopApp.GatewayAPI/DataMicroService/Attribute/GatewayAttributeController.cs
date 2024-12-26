using EshopApp.GatewayAPI.DataMicroService.Attribute.Models.RequestModels;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.DataMicroService.Attribute;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayAttributeController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayAttributeController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetAttributes(int amount)
    {
        try
        {
            //get the attributes
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"Attribute/Amount/{amount}");


            //validate that getting the attributes has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Attribute/Amount/{amount}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayAppAttribute>? attributes = JsonSerializer.Deserialize<List<GatewayAppAttribute>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(attributes);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttributeById(string id)
    {
        try
        {
            //get the attribute
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"Attribute/{id}");

            //validate that getting the attribute has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Attribute/{id}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppAttribute? attribute = JsonSerializer.Deserialize<GatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(attribute);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttribute(GatewayCreateAttributeRequestModel gatewayCreateAttributeRequestModel)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //create the attribute
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("Attribute", new { gatewayCreateAttributeRequestModel.Name });

            //validate that creating the attribute has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PostAsJsonAsync("Attribute", new { gatewayCreateAttributeRequestModel.Name });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppAttribute? attribute = JsonSerializer.Deserialize<GatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return CreatedAtAction(nameof(GetAttributeById), new { id = attribute!.Id }, attribute);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAttribute(GatewayUpdateAttributeRequestModel gatewayUpdateAttributeRequestModel)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //update the attribute
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PutAsJsonAsync("Attribute",
                new { Id = gatewayUpdateAttributeRequestModel.Id, Name = gatewayUpdateAttributeRequestModel.Name, VariantIds = gatewayUpdateAttributeRequestModel.VariantIds });

            //validate that updating the attribute has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PutAsJsonAsync("Attribute",
                    new { Id = gatewayUpdateAttributeRequestModel.Id, Name = gatewayUpdateAttributeRequestModel.Name, VariantIds = gatewayUpdateAttributeRequestModel.VariantIds });
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
    public async Task<IActionResult> DeleteAttribute(string id)
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
            HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");

            //validate that the authentication and authorization has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageProducts");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //delete the attribute
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.DeleteAsync($"Attribute/{id}");

            //validate that deleting the attribute has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"Attribute/{id}");
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
}
