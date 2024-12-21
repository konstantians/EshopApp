using EshopApp.GatewayAPI.HelperMethods;
using EshopApp.GatewayAPI.Models;
using EshopApp.GatewayAPI.Models.RequestModels.GatewayRoleControllerRequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Controllers;

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
            //get the roles
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync("Role");

            //validate that getting the roles has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync("Role");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
            //get the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Role/GetRoleById/{roleId}");

            //validate that getting the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Role/GetRoleById/{roleId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
            //get the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Role/GetRoleByName/{roleName}");

            //validate that getting the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Role/GetRoleByName/{roleName}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
            //get the user's roles
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Role/GetRolesOfUser/{userId}");

            //validate that getting the roles of the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Role/GetRolesOfUser/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
    public async Task<IActionResult> CreateRole([FromBody] GatewayApiCreateRoleRequestModel gatewayApiCreateRoleRequestModel)
    {
        try
        {
            //create the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Role", new { RoleName = gatewayApiCreateRoleRequestModel.RoleName, Claims = gatewayApiCreateRoleRequestModel.Claims });

            //validate that creating the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Role", new { RoleName = gatewayApiCreateRoleRequestModel.RoleName, Claims = gatewayApiCreateRoleRequestModel.Claims });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
            //delete the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.DeleteAsync($"Role/{roleId}");

            //validate that deleting the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.DeleteAsync($"Role/{roleId}");
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

    [HttpGet("GetUsersOfRole/{roleId}")]
    public async Task<IActionResult> GetUsersOfRole(string roleId)
    {
        try
        {
            //get the users of the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Role/GetUsersOfRole/{roleId}");

            //validate that getting the users of the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Role/GetUsersOfRole/{roleId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
    public async Task<IActionResult> AddRoleToUser([FromBody] GatewayApiAddRoleToUserRequestModel gatewayApiAddRoleToUserRequestModel)
    {
        try
        {
            //add the role to the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Role/addRoleToUser", new { RoleId = gatewayApiAddRoleToUserRequestModel.RoleId, UserId = gatewayApiAddRoleToUserRequestModel.UserId });

            //validate that adding the role to the user worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Role/addRoleToUser", new { RoleId = gatewayApiAddRoleToUserRequestModel.RoleId, UserId = gatewayApiAddRoleToUserRequestModel.UserId });
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

    [HttpPost("ReplaceRoleOfUser")]
    public async Task<IActionResult> ReplaceRoleOfUser([FromBody] GatewayApiReplaceRoleOfUserRequestModel gatewayApiReplaceRoleOfUserRequestModel)
    {
        try
        {
            //replace the role of the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Role/ReplaceRoleOfUser",
                new { CurrentRoleId = gatewayApiReplaceRoleOfUserRequestModel.CurrentRoleId, NewRoleId = gatewayApiReplaceRoleOfUserRequestModel.NewRoleId, UserId = gatewayApiReplaceRoleOfUserRequestModel.UserId });

            //validate that replacing the role of the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Role/ReplaceRoleOfUser",
                    new { CurrentRoleId = gatewayApiReplaceRoleOfUserRequestModel.CurrentRoleId, NewRoleId = gatewayApiReplaceRoleOfUserRequestModel.NewRoleId, UserId = gatewayApiReplaceRoleOfUserRequestModel.UserId });
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

    [HttpDelete("RemoveRoleFromUser/{userId}/role/{roleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
    {
        try
        {
            //remove the role from the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.DeleteAsync($"Role/RemoveRoleFromUser/{userId}/role/{roleId}");

            //validate that removing the role from the user has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.DeleteAsync($"Role/RemoveRoleFromUser/{userId}/role/{roleId}");
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

    [HttpGet("GetClaims")]
    public async Task<IActionResult> GetClaimsInSystem()
    {
        try
        {
            //get the claims of the system
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.GetAsync($"Role/GetClaims");

            //validate that getting the claims of the system has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.GetAsync($"Role/GetClaims");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

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
    public async Task<IActionResult> UpdateClaimsOfRole([FromBody] GatewayApiUpdateClaimsOfRoleRequestModel gatewayApiUpdateClaimsOfRoleRequestModel)
    {
        try
        {
            //update the claims of the role
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Role/UpdateClaimsOfRole",
                new { RoleId = gatewayApiUpdateClaimsOfRoleRequestModel.RoleId, NewClaims = gatewayApiUpdateClaimsOfRoleRequestModel.NewClaims });

            //validate that removing the claims from the role has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Role/UpdateClaimsOfRole",
                    new { RoleId = gatewayApiUpdateClaimsOfRoleRequestModel.RoleId, NewClaims = gatewayApiUpdateClaimsOfRoleRequestModel.NewClaims });
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
