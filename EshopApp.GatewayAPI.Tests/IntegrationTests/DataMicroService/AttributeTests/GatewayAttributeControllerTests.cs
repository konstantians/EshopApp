using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.AttributeTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.AttributeTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayAttributeControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenAttributeId;
    private string? _chosenProductId;
    private string? _chosenVariantId;
    private string? _otherAttributeId;
    private string? _otherAttributeName;

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

        //set the headers for the following operations
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        /*************** Product & Variant ***************/
        TestGatewayCreateProductRequestModel testCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testCreateProductRequestModel.Name = "OtherName";
        testCreateProductRequestModel.Code = "OtherCode";
        TestGatewayCreateVariantRequestModel testCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _chosenVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;

        /*************** Other Attribute ***************/
        TestGatewayCreateAttributeRequestModel testCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "OtherAttribute";
        response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherAttributeId = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherAttributeName = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateAttribute_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateAttribute_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel); //the property name is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateAttribute_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateAttribute_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateAttribute_ShouldFailAndReturnBadRequest_IfDuplicateAttributeName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = _otherAttributeName;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(70)]
    public async Task CreateAttribute_ShouldSucceedAndCreateAttribute()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateAttributeRequestModel testGatewayCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testGatewayCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testGatewayCreateAttributeRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppAttribute? testAttribute = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testAttribute.Should().NotBeNull();
        testAttribute!.Id.Should().NotBeNull();
        testAttribute!.Name.Should().NotBeNull().And.Be(testGatewayCreateAttributeRequestModel.Name);
        _chosenAttributeId = testAttribute.Id;
    }

    [Test, Order(80)]
    public async Task GetAttributes_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayAttribute/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetAttributes_ShouldSucceedAndReturnAttributes()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayAttribute/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppAttribute>? testAttributes = JsonSerializer.Deserialize<List<TestGatewayAppAttribute>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAttributes.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(100)]
    public async Task GetAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAttribute/{attributeId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusAttributeId = "bogusAttributeId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAttribute/{bogusAttributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task GetAttribute_ShouldSucceedAndReturnAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayAttribute/{attributeId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppAttribute? testAttribute = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAttribute.Should().NotBeNull();
        testAttribute!.Id.Should().NotBeNull().And.Be(attributeId);
        testAttribute!.Name.Should().NotBeNull();
    }

    [Test, Order(130)]
    public async Task UpdateAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task UpdateAttribute_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(150)]
    public async Task UpdateAttribute_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(160)]
    public async Task UpdateAttribute_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task UpdateAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = "bogusAttributeId";
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateAttribute_ShouldFailAndReturnBadRequest_IfDuplicateAttributeName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.Name = _otherAttributeName;
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateAttribute_ShouldSucceedAndUpdateAttribute()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayAttribute/{_chosenAttributeId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayAppAttribute? testAttribute = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testAttribute!.Name.Should().NotBeNull().And.Be(testGatewayUpdateAttributeRequestModel.Name);
        testAttribute.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(200)]
    public async Task UpdateAttribute_ShouldSucceedAndRemoveVariantFromAttribute()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateAttributeRequestModel testGatewayUpdateAttributeRequestModel = new TestGatewayUpdateAttributeRequestModel();
        testGatewayUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testGatewayUpdateAttributeRequestModel.VariantIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayAttribute", testGatewayUpdateAttributeRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayAttribute/{_chosenAttributeId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayAppAttribute? testAttribute = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testAttribute!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(210)]
    public async Task DeleteAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAttribute/{attributeId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteAttribute_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAttribute/{attributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(230)]
    public async Task DeleteAttribute_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAttribute/{attributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(240)]
    public async Task DeleteAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusAttributeId = "bogusAttributeId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAttribute/{bogusAttributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(250)]
    public async Task DeleteAttribute_ShouldSucceedAndDeleteAttribute()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayAttribute/{attributeId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayAttribute/{attributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetAttributes_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayAttribute/amount/10");

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
