using Microsoft.IdentityModel.Tokens;

namespace EshopApp.AuthLibraryAPI.Middlewares;

public class ApiKeyProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _apiKeys;

    public ApiKeyProtectionMiddleware(RequestDelegate next, List<string> apiKeys)
    {
        _next = next;
        _apiKeys = apiKeys;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        // Bypass API key check for specific endpoints(add the external login eventually, because it goes thought sign in
        if (path.StartsWithSegments("/api/authentication/ConfirmEmail") || path.StartsWithSegments("/api/authentication/ConfirmChangeEmail"))
        {
            await _next(context);
            return;
        }

        context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey);
        if (extractedApiKey.IsNullOrEmpty())
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { ErrorMessage = "X-API-KEYHeaderMissing" });
            return;
        }


        if (!_apiKeys.Contains<string>(extractedApiKey!))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { ErrorMessage = "InvalidAPIKey" });
            return;
        }

        await _next(context);
    }
}
