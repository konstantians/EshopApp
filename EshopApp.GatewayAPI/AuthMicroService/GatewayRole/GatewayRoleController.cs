using EshopApp.GatewayAPI.AuthMicroService.GatewayRole.Models.RequestModels;
using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayRole;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayRoleController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly IUtilityMethods _utilityMethods;
    private readonly IConfiguration _configuration;

    public GatewayRoleController(IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods, IConfiguration configuration)
    {
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        _utilityMethods = utilityMethods;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the roles
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Role"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayAppRole>? appRoles = JsonSerializer.Deserialize<List<GatewayAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(appRoles);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRoleById/{roleId}")]
    public async Task<IActionResult> GetRoleById(string roleId)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Role/GetRoleById/{roleId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            //appRole can not be null, because if it would have been caught in the 400 error codes check
            GatewayAppRole appRole = JsonSerializer.Deserialize<GatewayAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Ok(appRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRoleByName/{roleName}")]
    public async Task<IActionResult> GetRoleByName(string roleName)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Role/GetRoleByName/{roleName}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            //appRole can not be null, because if it would have been caught in the 400 error codes check
            GatewayAppRole appRole = JsonSerializer.Deserialize<GatewayAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Ok(appRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRolesOfUser/{userId}")]
    public async Task<IActionResult> GetRolesOfUser(string userId)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the user's roles
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Role/GetRolesOfUser/{userId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayAppRole> appRoles = JsonSerializer.Deserialize<List<GatewayAppRole>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Ok(appRoles);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] GatewayCreateRoleRequestModel gatewayApiCreateRoleRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //create the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.PostAsJsonAsync("Role", new { gatewayApiCreateRoleRequestModel.RoleName, gatewayApiCreateRoleRequestModel.Claims }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppRole appRole = JsonSerializer.Deserialize<GatewayAppRole>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return CreatedAtAction(nameof(GetRoleById), new { roleId = appRole!.Id }, appRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //delete the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.DeleteAsync($"Role/{roleId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUsersOfRole/{roleId}")]
    public async Task<IActionResult> GetUsersOfRole(string roleId)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the users of the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Role/GetUsersOfRole/{roleId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayAppUser> appUsers = JsonSerializer.Deserialize<List<GatewayAppUser>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Ok(appUsers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("AddRoleToUser")]
    public async Task<IActionResult> AddRoleToUser([FromBody] GatewayAddRoleToUserRequestModel gatewayApiAddRoleToUserRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //add the role to the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.PostAsJsonAsync("Role/addRoleToUser", new { gatewayApiAddRoleToUserRequestModel.RoleId, gatewayApiAddRoleToUserRequestModel.UserId }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("ReplaceRoleOfUser")]
    public async Task<IActionResult> ReplaceRoleOfUser([FromBody] GatewayReplaceRoleOfUserRequestModel gatewayApiReplaceRoleOfUserRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //replace the role of the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Role/ReplaceRoleOfUser",
                new { gatewayApiReplaceRoleOfUserRequestModel.CurrentRoleId, gatewayApiReplaceRoleOfUserRequestModel.NewRoleId, gatewayApiReplaceRoleOfUserRequestModel.UserId }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("RemoveRoleFromUser/{userId}/role/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //remove the role from the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.DeleteAsync($"Role/RemoveRoleFromUser/{userId}/role/{roleId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetClaims")]
    public async Task<IActionResult> GetClaimsInSystem()
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //get the claims of the system
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Role/GetClaims"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayClaim> CustomClaims = JsonSerializer.Deserialize<List<GatewayClaim>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Ok(CustomClaims);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("UpdateClaimsOfRole")]
    public async Task<IActionResult> UpdateClaimsOfRole([FromBody] GatewayUpdateClaimsOfRoleRequestModel gatewayApiUpdateClaimsOfRoleRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //update the claims of the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Role/UpdateClaimsOfRole",
                new { gatewayApiUpdateClaimsOfRoleRequestModel.RoleId, gatewayApiUpdateClaimsOfRoleRequestModel.NewClaims }));

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
