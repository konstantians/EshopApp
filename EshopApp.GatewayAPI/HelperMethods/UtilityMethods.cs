using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.GatewayAPI.HelperMethods;

public class UtilityMethods : IUtilityMethods
{
    public async Task AttemptToSendEmailAsync(HttpClient emailHttpClient, int retries, Dictionary<string, string> jsonObject)
    {
        if (retries == 0)
            return;

        HttpResponseMessage response = await emailHttpClient.PostAsJsonAsync("Emails", jsonObject);
        if (response.StatusCode == HttpStatusCode.OK)
            return;

        await Task.Delay(1000);
        retries--;
        await AttemptToSendEmailAsync(emailHttpClient, retries, jsonObject);
    }

    public string? SetDefaultHeadersForClient(bool includeJWTAuthenticationHeaders, HttpClient httpClient, string apiKey, string rateLimitingBypassCode, HttpRequest? httpRequest = null)
    {
        string? returnedAccessToken = null;
        if (includeJWTAuthenticationHeaders)
        {
            string authorizationHeader = httpRequest!.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            returnedAccessToken = accessToken;
        }

        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", rateLimitingBypassCode);
        return returnedAccessToken;
    }

    public bool CheckIfUrlIsTrusted(string redirectUrl, IConfiguration configuration)
    {
        List<string> trustedDomains = configuration["TrustedOrigins"]!.Split(" ").ToList();
        var redirectUri = new Uri(redirectUrl);

        foreach (string trustedDomain in trustedDomains)
        {
            var trustedUri = new Uri(trustedDomain);
            if (Uri.Compare(redirectUri, trustedUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
        }

        return false;
    }

    public async Task<bool> CheckIfMicroservicesFullyOnlineAsync(List<HttpClient> httpClients)
    {
        try
        {
            if (httpClients is null || !httpClients.Any())
                return true;

            foreach (HttpClient httpClient in httpClients)
            {
                var healthResponse = await httpClient.GetAsync("Health");
                if (healthResponse.StatusCode != HttpStatusCode.OK)
                    return false;
            }

            return true;
        }
        //the exception can happen if one of the microservices are not online. In this case just return false
        catch
        {
            return false;
        }
    }

    public async Task<IActionResult> CommonValidationForRequestClientErrorCodesAsync(HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        //in the case there is no body
        if (string.IsNullOrEmpty(responseBody))
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new UnauthorizedResult();
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return new BadRequestResult();
            else if (response.StatusCode == HttpStatusCode.NotFound)
                return new NotFoundResult();
            else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);

            return new BadRequestResult(); //this will probably never happen
        }

        //otherwise
        var keyValue = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
        keyValue!.TryGetValue("errorMessage", out object? errorMessageObject);
        string? errorMessage = errorMessageObject?.ToString() ?? null;
        keyValue!.TryGetValue("errors", out var errors);

        if (response.StatusCode == HttpStatusCode.Unauthorized && errorMessage is not null)
            return new UnauthorizedObjectResult(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new UnauthorizedResult();
        else if (response.StatusCode == HttpStatusCode.Forbidden && errorMessage is not null)
            return new ObjectResult(new { ErrorMessage = errorMessage }) { StatusCode = StatusCodes.Status403Forbidden };
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        else if (response.StatusCode == HttpStatusCode.BadRequest && errorMessage is not null)
            return new BadRequestObjectResult(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.BadRequest && errors is not null) //this is for request validation errors
            return new BadRequestObjectResult(new { Errors = errors });
        else if (response.StatusCode == HttpStatusCode.NotFound && errorMessage is not null)
            return new NotFoundObjectResult(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.NotFound)
            return new NotFoundResult();
        else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            return new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);

        return new BadRequestResult(); //this will probably never happen
    }
}