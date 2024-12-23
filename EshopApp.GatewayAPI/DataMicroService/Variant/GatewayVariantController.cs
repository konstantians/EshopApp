using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.DataMicroService.Variant.Models.RequestModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Text.Json;

namespace EshopApp.GatewayAPI.DataMicroService.Variant;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayVariantController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayVariantController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }


    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariants(int amount, bool includeDeactivated)
    {
        try
        {
            //get the variants
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"Variant/Amount/{amount}/includeSoftDeleted/{includeDeactivated}");

            //validate that getting the variants has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Variant/Amount/{amount}/includeSoftDeleted/{includeDeactivated}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayVariant>? variants = JsonSerializer.Deserialize<List<GatewayVariant>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(variants);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Id/{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariantById(string id, bool includeDeactivated)
    {
        try
        {
            //get the variant
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"Variant/Id/{id}/includeDeactivated/{includeDeactivated}");

            //validate that getting the variant has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Variant/Id/{id}/includeDeactivated/{includeDeactivated}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayVariant? variant = JsonSerializer.Deserialize<GatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Sku/{sku}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariantBySku(string sku, bool includeDeactivated)
    {
        try
        {
            //get the variant
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await dataHttpClient.GetAsync($"variant/sku/{sku}/includeDeactivated/{includeDeactivated}");

            //validate that getting the variant has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"variant/sku/{sku}/includeDeactivated/{includeDeactivated}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayVariant? variant = JsonSerializer.Deserialize<GatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant(GatewayCreateVariantRequestModel gatewayCreateVariantRequestModel)
    {
        try
        {
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

            //create the variant
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("Variant", gatewayCreateVariantRequestModel);

            //validate that creating the variant has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PostAsJsonAsync("Variant", gatewayCreateVariantRequestModel);
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayVariant? variant = JsonSerializer.Deserialize<GatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateVariant(GatewayUpdateVariantRequestModel gatewayUpdateVariantRequestModel)
    {
        try
        {
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

            //update the variant
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PutAsJsonAsync("Variant", gatewayUpdateVariantRequestModel);

            //validate that updating the variant has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.PutAsJsonAsync("Variant", gatewayUpdateVariantRequestModel);
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
    public async Task<IActionResult> DeleteVariant(string id)
    {
        try
        {
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

            //delete the variant
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.GetAsync($"Variant/{id}");

            //validate that deleting the variant has worked
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.GetAsync($"Variant/{id}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            if (response.StatusCode == HttpStatusCode.OK) //this can happen if the variant exists in order and thus it only becomes deactivated/soft deleted
                return Ok(new { WarningMessage = "NoErrorButNotFullyDeleted" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
