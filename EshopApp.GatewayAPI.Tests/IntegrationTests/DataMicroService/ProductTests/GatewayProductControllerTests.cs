using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.AttributeTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CategoryTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayProductControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenProductId;
    private string? _otherProductName;
    private string? _otherProductCode;
    private string? _otherVariantSku;
    private string? _firstCategoryId;
    private string? _secondCategoryId;
    private string? _thirdCategoryId;
    private string? _chosenAttributeId;
    private string? _chosenImageId;

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

        //set the correct api key and the correct access token for the following
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        /*************** Other Product ***************/
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
        _otherProductName = testCreateProductRequestModel.Name;
        _otherProductCode = testCreateProductRequestModel.Code;
        _otherVariantSku = testCreateVariantRequestModel.SKU;

        /*************** Category 1 ***************/
        TestGatewayCreateCategoryRequestModel testCreateCategoryRequestModel = new TestGatewayCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "Category1";
        response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testCreateCategoryRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _firstCategoryId = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Category 2 ***************/
        testCreateCategoryRequestModel.Name = "Category2";
        response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testCreateCategoryRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _secondCategoryId = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Category 3 ***************/
        testCreateCategoryRequestModel.Name = "Category3";
        response = await httpClient.PostAsJsonAsync("api/gatewayCategory", testCreateCategoryRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _thirdCategoryId = JsonSerializer.Deserialize<TestGatewayCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Attribute ***************/
        TestGatewayCreateAttributeRequestModel testCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";
        response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenAttributeId = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Image ***************/
        TestGatewayCreateImageRequestModel testCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testCreateImageRequestModel.Name = "Image";
        testCreateImageRequestModel.ImagePath = "Path";
        response = await httpClient.PostAsJsonAsync("api/gatewayImage", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenImageId = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateProduct_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInProductEntity()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel); //The name value of the product is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInProductVariantEntity()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = -1;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel); //The UnitsInStock value of the variant can not be negative and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(50)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfVariantEntityIsMissing()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel); //The Variant Property is mising and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(60)]
    public async Task CreateProduct_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel); //The UnitsInStock value of the variant can not be negative and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(70)]
    public async Task CreateProduct_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel); //The UnitsInStock value of the variant can not be negative and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(80)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = _otherProductName;
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(90)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductCode()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = _otherProductCode;
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateProductCode");
    }

    [Test, Order(100)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateVariantSKU()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = _otherVariantSku;
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(110)]
    public async Task CreateProduct_ShouldSucceedAndCreateProduct()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProduct";
        testGatewayCreateProductRequestModel.Code = "MyCode";
        testGatewayCreateProductRequestModel.Description = "My product description";
        testGatewayCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;


        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testProduct.Should().NotBeNull();
        testProduct!.Id.Should().NotBeNull();
        testProduct!.Name.Should().NotBeNull().And.Be(testGatewayCreateProductRequestModel.Name);
        testProduct!.Code.Should().NotBeNull().And.Be(testGatewayCreateProductRequestModel.Code);
        testProduct!.Description.Should().NotBeNull().And.Be(testGatewayCreateProductRequestModel.Description);
        testProduct!.Categories.Should().NotBeNull().And.HaveCount(2);
        testProduct!.Variants[0]!.SKU.Should().Be(testGatewayCreateVariantRequestModel.SKU);
        testProduct!.Variants[0]!.Price.Should().Be(testGatewayCreateVariantRequestModel.Price);
        testProduct!.Variants[0]!.UnitsInStock.Should().Be(testGatewayCreateVariantRequestModel.UnitsInStock);
        testProduct!.Variants[0]!.ProductId.Should().NotBeNull().And.Be(testProduct.Id);
        testProduct!.Variants[0]!.Attributes.Should().NotBeNull().And.HaveCount(1);
        testProduct!.Variants[0]!.VariantImages.Should().NotBeNull().And.HaveCount(1);
        _chosenProductId = testProduct.Id;
    }

    [Test, Order(120)]
    public async Task GetProducts_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayProduct/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(130)]
    public async Task GetProducts_ShouldSucceedAndReturnProducts()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayProduct/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayProduct>? testProducts = JsonSerializer.Deserialize<List<TestGatewayProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testProducts.Should().NotBeNull().And.HaveCount(2); //one we created on the setup and one in the previous tests
    }

    [Test, Order(140)]
    public async Task GetProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayProduct/{productId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task GetProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusProductId = "bogusProductId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayProduct/{bogusProductId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(160)]
    public async Task GetProduct_ShouldSucceedAndReturnProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayProduct/{productId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testProduct.Should().NotBeNull();
        testProduct!.Id.Should().NotBeNull().And.Be(productId);
        testProduct!.Name.Should().NotBeNull();
        testProduct.Code.Should().NotBeNull();
        testProduct.Description.Should().NotBeNull();
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(1);
        testProduct.Variants[0].VariantImages.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(170)]
    public async Task UpdateProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(180)]
    public async Task UpdateProduct_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(190)]
    public async Task UpdateProduct_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(200)]
    public async Task UpdateProduct_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(210)]
    public async Task UpdateProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = "BogusProductId";
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(220)]
    public async Task UpdateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = _otherProductName;
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(230)]
    public async Task UpdateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductCode()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = _otherProductCode;
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testGatewayUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateProductCode");
    }

    [Test, Order(240)]
    public async Task UpdateProduct_ShouldSucceedAndUpdateProduct()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.Code = "MyCodeUpdated";
        testGatewayUpdateProductRequestModel.Name = "MyProductUpdated";
        testGatewayUpdateProductRequestModel.Description = "My description updated";
        testGatewayUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayProduct/{_chosenProductId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testProduct!.Code.Should().NotBeNull().And.Be("MyCodeUpdated");
        testProduct!.Name.Should().NotBeNull().And.Be("MyProductUpdated");
        testProduct!.Description.Should().NotBeNull().And.Be("My description updated");
        testProduct!.Categories.Should().NotBeNull().And.HaveCount(1);
        testProduct!.Categories[0].Id.Should().Be(_thirdCategoryId);
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(250)]
    public async Task UpdateProduct_ShouldSucceedAndRemoveVariantFromProduct()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateProductRequestModel testGatewayUpdateProductRequestModel = new TestGatewayUpdateProductRequestModel();
        testGatewayUpdateProductRequestModel.Id = _chosenProductId;
        testGatewayUpdateProductRequestModel.VariantIds = new List<string>();

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayProduct", testGatewayUpdateProductRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayProduct/{_chosenProductId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testProduct!.Code.Should().NotBeNull().And.Be("MyCodeUpdated");
        testProduct!.Name.Should().NotBeNull().And.Be("MyProductUpdated");
        testProduct!.Description.Should().NotBeNull().And.Be("My description updated");
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(260)]
    public async Task DeleteProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayProduct/{productId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(270)]
    public async Task DeleteProduct_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayProduct/{productId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(280)]
    public async Task DeleteProduct_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayProduct/{productId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(290)]
    public async Task DeleteProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusProductId = "bogusProductId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayProduct/{bogusProductId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(300)]
    public async Task DeleteProduct_ShouldSucceedAndDeleteProduct()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayProduct/{productId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayProduct/{productId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetProducts_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayProduct/amount/10/includeDeactivated/true");

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
