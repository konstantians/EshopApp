using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CategoryTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CategoryTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayCategoryControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenProductId;
    private string? _chosenCategoryId;

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

        /*************** Product & Variant ***************/
        TestGatewayCreateProductRequestModel testCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestGatewayCreateVariantRequestModel testCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "ProductVariatSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken); //we need the admin access token, because it has the claim CanManageProducts
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testCreateProductRequestModel);
        string responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateCategory_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCategory_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCategory_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusToken");
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateCategory_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken); //user does not have claim CanManageProducts
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateCategory_ShouldSucceedAndCreateCategory()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCategory? testCategory = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCategory.Should().NotBeNull();
        testCategory!.Id.Should().NotBeNull();
        testCategory!.Name.Should().NotBeNull().And.Be(testGatewayCreateCategoryRequestModel.Name);
        _chosenCategoryId = testCategory.Id;
    }

    [Test, Order(70)]
    public async Task CreateCategory_ShouldFailAndReturnBadRequest_IfDuplicateCategoryName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testGatewayCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(80)]
    public async Task GetCategories_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCategory/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetCategories_ShouldSucceedAndReturnCategories()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCategory/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayCategory>? testCategories = JsonSerializer.Deserialize<List<TestGatewayCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategories.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(100)]
    public async Task GetCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCategory/{categoryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCategoryId = "bogusCategoryId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCategory/{bogusCategoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task GetCategory_ShouldSucceedAndReturnCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCategory/{categoryId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCategory? testCategory = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategory.Should().NotBeNull();
        testCategory!.Id.Should().NotBeNull().And.Be(categoryId);
        testCategory!.Name.Should().NotBeNull();
    }

    [Test, Order(130)]
    public async Task UpdateCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task UpdateCategory_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(150)]
    public async Task UpdateCategory_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(160)]
    public async Task UpdateCategory_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task UpdateCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = "bogusCategoryId";
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateCategory_ShouldFailAndReturnBadRequest_IfDuplicateCategoryName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.Name = "AnotherCategory";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        TestGatewayCreateCategoryRequestModel testGatewayCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testGatewayCreateCategoryRequestModel.Name = "AnotherCategory";
        await httpClient.PostAsJsonAsync("api/gatewayCategory/", testGatewayCreateCategoryRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateCategory_ShouldSucceedAndUpdateCategory()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCategory/{_chosenCategoryId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCategory? testGatewayCategory = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testGatewayCategory!.Name.Should().NotBeNull().And.Be(testGatewayUpdateCategoryRequestModel.Name);
        testGatewayCategory.Products.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(200)]
    public async Task UpdateCategory_ShouldSucceedAndRemoveProductFromCategory()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCategoryRequestModel testGatewayUpdateCategoryRequestModel = new TestGatewayUpdateCategoryRequestModel();
        testGatewayUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testGatewayUpdateCategoryRequestModel.ProductIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCategory", testGatewayUpdateCategoryRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCategory/{_chosenCategoryId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCategory? testGatewayCategory = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testGatewayCategory!.Products.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(210)]
    public async Task DeleteCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCategory/{categoryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteCategory_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCategory/{categoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(230)]
    public async Task DeleteCategory_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCategory/{categoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(240)]
    public async Task DeleteCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusCategoryId = "bogusCategoryId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCategory/{bogusCategoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(250)]
    public async Task DeleteCategory_ShouldSucceedAndDeleteCategory()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCategory/{categoryId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayCategory/{categoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetCategories_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayCategory/amount/10");

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
