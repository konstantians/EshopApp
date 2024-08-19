using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.ResponseModels;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Azure;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class CommonProcedures
{
    internal static async Task<(string userId, string adminId, string userAccessToken, string managerAccessToken, string adminAccessToken)> commonAdminManagerAndUserSetup(HttpClient httpClient)
    {
        //set up the admin
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel(email: "admin@hotmail.com", password: "0XfN725l5EwSTIk!", rememberMe: true);
        HttpResponseMessage adminResponse = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? adminAccessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(adminResponse, "accessToken");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        HttpResponseMessage getAdminUserIdResponse = await httpClient.GetAsync("api/authentication/getuserbyaccesstoken");
        string? getAdminUserIdResponseBody = await getAdminUserIdResponse.Content.ReadAsStringAsync();
        TestAppUser? admin = JsonSerializer.Deserialize<TestAppUser>(getAdminUserIdResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        string adminId = admin!.Id;

        //set up the manager
        testSignInRequestModel = new TestSignInRequestModel(email: "manager@hotmail.com", password: "CIiyyBRXjTGac7j!", rememberMe: true);
        HttpResponseMessage managerResponse = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? managerAccessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(managerResponse, "accessToken");

        //set up the user
        TestSignUpRequestModel testSignUpRequestModel = new TestSignUpRequestModel(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2016!");
        HttpResponseMessage userResponse = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpRequestModel);
        string? responseBody = await userResponse.Content.ReadAsStringAsync();
        TestSignUpResponseModel? userSignupResponseModel = JsonSerializer.Deserialize<TestSignUpResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        string userId = userSignupResponseModel!.UserId!;

        var userConfirmResponse = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userSignupResponseModel!.UserId}&confirmEmailToken={WebUtility.UrlEncode(userSignupResponseModel.ConfirmationToken)}");
        string? userAccessToken = await JsonParsingHelperMethods.GetSingleStringValueFromBody(userConfirmResponse, "accessToken");
    
        return (userId, adminId, userAccessToken!, managerAccessToken!, adminAccessToken!);
    }

    internal static void SetDefaultHttpHeaders(HttpClient httpClient, string? apiKey, string? accessToken)
    {
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    internal static void EntityNotFoundChecks(HttpResponseMessage response, string? errorMessage)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Be(errorMessage);
    }

    internal static void ApiKeyIsMissingChecks(HttpResponseMessage response, string? errorMessage)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }
}
