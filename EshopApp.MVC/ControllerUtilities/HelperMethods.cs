using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;

namespace EshopApp.MVC.ControllerUtilities;

public static class HelperMethods
{
    public static bool BasicTokenValidation(HttpRequest httpRequest)
    {
        var accessToken = httpRequest.Cookies["EshopAppAuthenticationCookie"];
        if (string.IsNullOrEmpty(accessToken))
            return false;

        //basic check for possible tempering
        string[] parts = accessToken.Split('.');
        if (parts.Length != 3)
            return false;

        return true;
    }

    //Think about this
    public static List<(string Type, string Value)> GetClaimsFromToken(HttpRequest httpRequest)
    {
        var token = httpRequest.Cookies["EshopAppAuthenticationCookie"];
        var handler = new JwtSecurityTokenHandler();

        try
        {
            if (!handler.CanReadToken(token))
                return new List<(string, string)>(); // or throw

            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims
                .Select(c => (Type: c.Type, Value: c.Value))
                .ToList();
        }
        catch
        {
            // Log error if needed
            return new List<(string, string)>();
        }
    }

    public static async Task<IActionResult?> CommonErrorValidation(Controller controller, ILogger logger, HttpResponseMessage response, string? responseBody, string redirectToAction,
        string redirectToController, object? routeValues = null, bool responseBodyWasPassedIn = false)
    {
        //this deals with 5xx errors
        if (response.StatusCode == HttpStatusCode.InternalServerError)
            return controller.RedirectToAction("Error500");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            return controller.RedirectToAction("Error503");
        else if ((int)response.StatusCode >= 500)
            return controller.RedirectToAction("Error500");

        //this deals with 4xx errors with empty response bodies
        responseBody = responseBodyWasPassedIn ? responseBody : await response.Content.ReadAsStringAsync();
        if ((int)response.StatusCode >= 400 && string.IsNullOrEmpty(responseBody))
        {
            controller.TempData["UnknownError"] = true;
            return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
        }
        //this deals with 4xx errors with non-empty response bodies
        else if ((int)response.StatusCode >= 400)
        {
            try
            {
                var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody!);
                responseObject!.TryGetValue("errorMessage", out string? errorMessage);
                controller.TempData[errorMessage ?? "UnknownError"] = true;
                return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Unexpected front end error");
                controller.TempData["UnknownError"] = true;
                return controller.RedirectToAction(redirectToAction, redirectToController, routeValues: routeValues);
            }
        }

        return null;
    }
}
