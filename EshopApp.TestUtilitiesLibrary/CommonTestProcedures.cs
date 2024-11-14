using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;

namespace EshopApp.TestUtilitiesLibrary;
public class CommonTestProcedures
{
    public static void SetDefaultHttpHeaders(HttpClient httpClient, string? apiKey, string? accessToken)
    {
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static void EntityNotFoundChecks(HttpResponseMessage response, string? errorMessage)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be(errorMessage);
    }

    public static void ApiKeyIsMissingChecks(HttpResponseMessage response, string? errorMessage)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }
}
