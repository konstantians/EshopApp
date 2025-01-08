using Microsoft.AspNetCore.Mvc.Testing;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.RefundTests;
internal class GatewayRefundControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        //set up microservices and do cleanup
        await HelperMethods.CommonProcedures.CommonProcessManagementDatabaseAndEmailCleanupAsync(false);

        (_userAccessToken, _managerAccessToken, _adminAccessToken) = await HelperMethods.CommonProcedures.CommonUsersSetupAsync(httpClient, waitTimeInMillisecond);

        //TODO set up here the creation of the order... 
        //The question is how? In order for an order to be created a session needs to be created
        //this can be done, after the typical thing is that the handle thing comes in place but in this case I will take it from there
        //so here the only thing we need to do is create the check out cart
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        await HelperMethods.CommonProcedures.CommonProcessManagementDatabaseAndEmailCleanupAsync(true);

        httpClient.Dispose();
    }
}
