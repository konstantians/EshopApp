namespace EshopApp.GatewayAPI.Middlewares;

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
        context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey);
        if (string.IsNullOrEmpty(extractedApiKey))
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
