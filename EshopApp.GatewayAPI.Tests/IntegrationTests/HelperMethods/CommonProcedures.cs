using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAdminTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.Utilities;
using System.Net.Http.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.HelperMethods;
internal class CommonProcedures
{
    internal static async Task<(string userAccessToken, string managerAccessToken, string adminAccessToken)> CommonUsersSetupAsync(HttpClient httpClient, int waitTimeInMilliseconds = 6000)
    {
        //sign up simple user
        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = "kinnaskonstantinos0@gmail.com";
        signUpModel.PhoneNumber = "6943655624";
        signUpModel.Password = "Kinas2016!";
        signUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";
        await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        await Task.Delay(waitTimeInMilliseconds);
        string? confirmationEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);
        try
        {
            using HttpClient tempHttpClient = new HttpClient();
            await tempHttpClient.GetAsync(confirmationEmailLink);
        }
        catch { }

        //get user access token
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayApiSignInRequestModel.Password = "Kinas2016!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? userAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        //get manager access token
        testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "manager@hotmail.com";
        testGatewayApiSignInRequestModel.Password = "CIiyyBRXjTGac7j!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? managerAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        //get admin access
        testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "admin@hotmail.com";
        testGatewayApiSignInRequestModel.Password = "0XfN725l5EwSTIk!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        string? adminAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        return (userAccessToken!, managerAccessToken!, adminAccessToken!);
    }

    internal static async Task CommonProcessManagementDatabaseAndEmailCleanupAsync(bool terminateTheProcesses, bool startAuthService = true, bool startDataService = true, bool startEmailService = true, bool startTransactionService = false, bool startMvcClient = false)
    {
        if (!terminateTheProcesses)
        {
            IProcessManagementService processManagementService = new ProcessManagementService();
            processManagementService.BuildAndRunApplication(startAuthService, startDataService, startEmailService, startTransactionService, startMvcClient);
        }
        else
        {
            IProcessManagementService processManagementService = new ProcessManagementService();
            processManagementService.TerminateApplication();
        }

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );

        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();
    }
}
