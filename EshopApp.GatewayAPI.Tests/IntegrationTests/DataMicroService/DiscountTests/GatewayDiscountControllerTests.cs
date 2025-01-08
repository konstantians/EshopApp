using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.DiscountTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.DiscountTests;

internal class GatewayDiscountControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenDiscountId;
    private string? _otherDiscountName;
    private string? _chosenVariantId;

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

        //set the headers that are required by the following operations
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        /*************** Product & Variant ***************/
        TestGatewayCreateProductRequestModel testCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestGatewayCreateVariantRequestModel testCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "VariantSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;

        /*************** Other Discount ***************/
        TestGatewayCreateDiscountRequestModel testCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "OtherDiscount";
        testCreateDiscountRequestModel.Percentage = 10;
        response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testCreateDiscountRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherDiscountName = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateDiscount_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = "MyDiscount";
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = "MyDiscount";
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateDiscount_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel); //here name is missing, which makes the request model invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateDiscount_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = "MyDiscount";
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateDiscount_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = "MyDiscount";
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateDiscount_ShouldFailAndReturnBadRequest_IfDuplicateDiscountName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = _otherDiscountName;
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(70)]
    public async Task CreateDiscount_ShouldSucceedAndCreateDiscount()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateDiscountRequestModel testGatewayCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testGatewayCreateDiscountRequestModel.Name = "MyDiscount";
        testGatewayCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testGatewayCreateDiscountRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayDiscount? testDiscount = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testDiscount.Should().NotBeNull();
        testDiscount!.Id.Should().NotBeNull();
        testDiscount!.Name.Should().NotBeNull().And.Be(testGatewayCreateDiscountRequestModel.Name);
        testDiscount!.Percentage.Should().Be(testGatewayCreateDiscountRequestModel.Percentage);
        _chosenDiscountId = testDiscount.Id;
    }

    [Test, Order(80)]
    public async Task GetDiscounts_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayDiscount/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetDiscounts_ShouldSucceedAndReturnDiscounts()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayDiscount/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayDiscount>? testDiscounts = JsonSerializer.Deserialize<List<TestGatewayDiscount>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testDiscounts.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(100)]
    public async Task GetDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayDiscount/{discountId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusDiscountId = "bogusDiscountId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayDiscount/{bogusDiscountId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task GetDiscount_ShouldSucceedAndReturnDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayDiscount/{discountId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayDiscount? testDiscount = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testDiscount.Should().NotBeNull();
        testDiscount!.Id.Should().NotBeNull().And.Be(discountId);
        testDiscount!.Name.Should().NotBeNull();
    }

    [Test, Order(130)]
    public async Task UpdateDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task UpdateDiscount_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(150)]
    public async Task UpdateDiscount_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(160)]
    public async Task UpdateDiscount_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task UpdateDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = "bogusDiscountId";
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateDiscount_ShouldFailAndReturnBadRequest_IfDuplicateDiscountName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.Name = _otherDiscountName;
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateDiscount_ShouldSucceedAndUpdateDiscount()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testGatewayUpdateDiscountRequestModel.Percentage = 50;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayDiscount/{_chosenDiscountId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayDiscount? testDiscount = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testDiscount!.Name.Should().NotBeNull().And.Be(testGatewayUpdateDiscountRequestModel.Name);
        testDiscount!.Percentage.Should().Be(testGatewayUpdateDiscountRequestModel.Percentage);
        testDiscount.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(200)]
    public async Task UpdateDiscount_ShouldSucceedAndRemoveVariantFromDiscount()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateDiscountRequestModel testGatewayUpdateDiscountRequestModel = new TestGatewayUpdateDiscountRequestModel();
        testGatewayUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testGatewayUpdateDiscountRequestModel.VariantIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayDiscount", testGatewayUpdateDiscountRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayDiscount/{_chosenDiscountId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayDiscount? testDiscount = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testDiscount!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(210)]
    public async Task DeleteDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayDiscount/{discountId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteDiscount_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayDiscount/{discountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(230)]
    public async Task DeleteDiscount_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayDiscount/{discountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(240)]
    public async Task DeleteDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusDiscountId = "bogusDiscountId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayDiscount/{bogusDiscountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(250)]
    public async Task DeleteDiscount_ShouldSucceedAndDeleteDiscount()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayDiscount/{discountId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayDiscount/{discountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetDiscounts_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayDiscount/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        await HelperMethods.CommonProcedures.CommonProcessManagementDatabaseAndEmailCleanupAsync(true);

        httpClient.Dispose();
    }
}
