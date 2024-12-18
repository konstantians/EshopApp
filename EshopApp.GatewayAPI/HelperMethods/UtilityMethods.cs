using System.Net;
using System.Net.Http.Headers;

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
}