﻿
using Microsoft.AspNetCore.Mvc;

namespace EshopApp.GatewayAPI.HelperMethods;

public interface IUtilityMethods
{
    Task AttemptToSendEmailAsync(HttpClient emailHttpClient, int retries, Dictionary<string, string> jsonObject);
    Task<bool> CheckIfMicroservicesFullyOnlineAsync(List<HttpClient> httpClients);
    bool CheckIfUrlIsTrusted(string redirectUrl, IConfiguration configuration);
    Task<IActionResult> CommonValidationForRequestClientErrorCodesAsync(HttpResponseMessage response);
    string? SetDefaultHeadersForClient(bool includeJWTAuthenticationHeaders, HttpClient httpClient, string apiKey, string rateLimitingBypassCode, HttpRequest? httpRequest = null);
}