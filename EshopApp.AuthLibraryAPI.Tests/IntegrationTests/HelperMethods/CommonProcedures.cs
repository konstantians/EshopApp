using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AuthenticationModels;
using EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models.ResponseModels.AdminModels;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class CommonProcedures
{
    internal static async Task<(string userId, string adminId, string userAccessToken, string managerAccessToken, string adminAccessToken)> CommonAdminManagerAndUserSetup(HttpClient httpClient)
    {
        //set up the admin
        TestSignInRequestModel testSignInRequestModel = new TestSignInRequestModel(email: "admin@hotmail.com", password: "0XfN725l5EwSTIk!", rememberMe: true);
        HttpResponseMessage adminResponse = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? adminAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(adminResponse, "accessToken");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        HttpResponseMessage getAdminUserIdResponse = await httpClient.GetAsync("api/authentication/getuserbyaccesstoken");
        string? getAdminUserIdResponseBody = await getAdminUserIdResponse.Content.ReadAsStringAsync();
        TestAppUser? admin = JsonSerializer.Deserialize<TestAppUser>(getAdminUserIdResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        string adminId = admin!.Id;

        //set up the manager
        testSignInRequestModel = new TestSignInRequestModel(email: "manager@hotmail.com", password: "CIiyyBRXjTGac7j!", rememberMe: true);
        HttpResponseMessage managerResponse = await httpClient.PostAsJsonAsync("api/authentication/signin", testSignInRequestModel);
        string? managerAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(managerResponse, "accessToken");

        //set up the user
        TestSignUpRequestModel testSignUpRequestModel = new TestSignUpRequestModel(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2016!");
        HttpResponseMessage userResponse = await httpClient.PostAsJsonAsync("api/authentication/signup", testSignUpRequestModel);
        string? responseBody = await userResponse.Content.ReadAsStringAsync();
        TestSignUpResponseModel? userSignupResponseModel = JsonSerializer.Deserialize<TestSignUpResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        string userId = userSignupResponseModel!.UserId!;

        var userConfirmResponse = await httpClient.GetAsync($"api/authentication/confirmemail?userid={userSignupResponseModel!.UserId}&confirmEmailToken={WebUtility.UrlEncode(userSignupResponseModel.ConfirmationToken)}");
        string? userAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(userConfirmResponse, "accessToken");

        return (userId, adminId, userAccessToken!, managerAccessToken!, adminAccessToken!);
    }
}
