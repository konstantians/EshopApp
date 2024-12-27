using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.AttributeTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.DiscountTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayVariantControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenVariantId;
    private string? _chosenVariantSku;
    private string? _otherVariantSku;
    private string? _otherVariantId;
    private string? _chosenProductId;
    private string? _chosenImageId;
    private string? _otherImageId;
    private string? _chosenAttributeId;
    private string? _otherAttributeId;
    private string? _chosenDiscountId;

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

        /*************** Product & Variant ***************/
        TestGatewayCreateProductRequestModel testCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestGatewayCreateVariantRequestModel testCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;
        _otherVariantSku = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].SKU;

        /*************** Image ***************/
        TestGatewayCreateImageRequestModel testCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testCreateImageRequestModel.Name = "Image";
        testCreateImageRequestModel.ImagePath = "Path";
        response = await httpClient.PostAsJsonAsync("api/gatewayImage", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenImageId = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other Image ***************/
        testCreateImageRequestModel.Name = "OtherImage";
        testCreateImageRequestModel.ImagePath = "OtherPath";
        response = await httpClient.PostAsJsonAsync("api/gatewayImage", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherImageId = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Discount ***************/
        TestGatewayCreateDiscountRequestModel testCreateDiscountRequestModel = new TestGatewayCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 10;
        response = await httpClient.PostAsJsonAsync("api/gatewayDiscount", testCreateDiscountRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenDiscountId = JsonSerializer.Deserialize<TestGatewayDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;


        /*************** Attribute ***************/
        TestGatewayCreateAttributeRequestModel testCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";
        response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenAttributeId = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other Attribute ***************/
        testCreateAttributeRequestModel = new TestGatewayCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "OtherAttribute";
        response = await httpClient.PostAsJsonAsync("api/gatewayAttribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherAttributeId = JsonSerializer.Deserialize<TestGatewayAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateVariant_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateVariant_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel); //here productId is missing, which is required, so the model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateVariant_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel); //here productId is missing, which is required, so the model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateVariant_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel); //here productId is missing, which is required, so the model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateVariant_ShouldFailAndReturnNotFound_IfInvalidProductId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = "bogusProductId";
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidProductIdWasGiven");
    }

    [Test, Order(70)]
    public async Task CreateVariant_ShouldFailAndReturnBadRequest_IfDuplicateVariantSKU()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = _otherVariantSku;
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(80)]
    public async Task CreateVariant_ShouldSucceedAndCreateVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 30m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = _chosenProductId;
        testGatewayCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testGatewayCreateVariantImageRequestModel = new TestGatewayCreateVariantImageRequestModel();
        testGatewayCreateVariantImageRequestModel.IsThumbNail = true;
        testGatewayCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testGatewayCreateVariantRequestModel.VariantImageRequestModels.Add(testGatewayCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage productGetResponse = await httpClient.GetAsync($"api/gatewayProduct/{_chosenProductId}/includeDeactivated/true");
        string? productResponseBody = await productGetResponse.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(productResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull();
        testVariant!.SKU.Should().NotBeNull().And.Be(testGatewayCreateVariantRequestModel.SKU);
        testVariant!.Price.Should().Be(testGatewayCreateVariantRequestModel.Price);
        testVariant!.UnitsInStock.Should().Be(testGatewayCreateVariantRequestModel.UnitsInStock);
        testVariant!.IsThumbnailVariant.Should().Be(testGatewayCreateVariantRequestModel.IsThumbnailVariant);

        testVariant!.Discount.Should().NotBeNull();
        testVariant!.Discount!.Id.Should().NotBeNull().And.Be(_chosenDiscountId);

        testVariant!.Attributes.Should().HaveCount(1);
        testVariant!.Attributes[0].Should().NotBeNull();
        testVariant!.Attributes[0].Id.Should().Be(_chosenAttributeId);

        testVariant!.VariantImages.Should().HaveCount(1);
        testVariant!.VariantImages[0].Should().NotBeNull();
        testVariant!.VariantImages[0].IsThumbNail.Should().Be(testGatewayCreateVariantImageRequestModel.IsThumbNail);
        testVariant!.VariantImages[0].ImageId.Should().NotBeNull().And.Be(_chosenImageId);

        testProduct!.Variants.Should().HaveCount(2);
        testProduct!.Variants.FirstOrDefault(variant => variant.Id == testVariant.Id)!.IsThumbnailVariant.Should()!.BeTrue();
        testProduct!.Variants.FirstOrDefault(variant => variant.Id == _otherVariantId)!.IsThumbnailVariant.Should()!.BeFalse();

        _chosenVariantId = testVariant.Id;
        _chosenVariantSku = testVariant.SKU;
    }

    [Test, Order(90)]
    public async Task GetVariants_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayVariant/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(100)]
    public async Task GetVariants_ShouldSucceedAndReturnVariants()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayVariant/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayVariant>? testCategories = JsonSerializer.Deserialize<List<TestGatewayVariant>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategories.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(110)]
    public async Task GetVariantById_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/id/{variantId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task GetVariantById_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusVariantId = "bogusVariantId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/id/{bogusVariantId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(130)]
    public async Task GetVariantById_ShouldSucceedAndReturnVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/id/{variantId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull().And.Be(variantId);
        testVariant!.SKU.Should().NotBeNull().And.Be(_chosenVariantSku);
        testVariant!.ProductId.Should().NotBeNull().And.Be(_chosenProductId);
        testVariant!.DiscountId.Should().NotBeNull().And.Be(_chosenDiscountId);
    }

    [Test, Order(140)]
    public async Task GetVariantBySku_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string variantSku = _chosenVariantSku!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/sku/{_chosenVariantSku}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task GetVariantBySku_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusVariantSku = "bogusVariantSku";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/sku/{bogusVariantSku}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(160)]
    public async Task GetVariantBySku_ShouldSucceedAndReturnVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string variantSku = _chosenVariantSku!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayVariant/sku/{variantSku}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull().And.Be(_chosenVariantId);
        testVariant!.SKU.Should().NotBeNull().And.Be(variantSku);
        testVariant!.ProductId.Should().NotBeNull().And.Be(_chosenProductId);
        testVariant!.DiscountId.Should().NotBeNull().And.Be(_chosenDiscountId);
    }

    [Test, Order(170)]
    public async Task UpdateVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(180)]
    public async Task UpdateVariant_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(190)]
    public async Task UpdateVariant_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(200)]
    public async Task UpdateVariant_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(210)]
    public async Task UpdateVariant_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = "bogusVariantId";
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(220)]
    public async Task UpdateVariant_ShouldFailAndReturnBadRequest_IfDuplicateVariantSku()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.SKU = _otherVariantSku;
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(230)]
    public async Task UpdateVariant_ShouldSucceedAndUpdateVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.SKU = "UpdatedSku";
        testGatewayUpdateVariantRequestModel.Price = 20;
        testGatewayUpdateVariantRequestModel.IsThumbnailVariant = false;
        testGatewayUpdateVariantRequestModel.UnitsInStock = 30;
        testGatewayUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testGatewayUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testGatewayUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testGatewayUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage productGetResponse = await httpClient.GetAsync($"api/gatewayProduct/{_chosenProductId}/includeDeactivated/true");
        string? productResponseBody = await productGetResponse.Content.ReadAsStringAsync();
        TestGatewayProduct? testProduct = JsonSerializer.Deserialize<TestGatewayProduct>(productResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.SKU.Should().NotBeNull().And.Be(testGatewayUpdateVariantRequestModel.SKU);
        testVariant!.Price.Should().Be(testGatewayUpdateVariantRequestModel.Price);
        testVariant.IsThumbnailVariant.Should().Be(testGatewayUpdateVariantRequestModel.IsThumbnailVariant);
        testVariant.UnitsInStock.Should().Be(testGatewayUpdateVariantRequestModel.UnitsInStock);

        testVariant.Discount.Should().NotBeNull();
        testVariant.Discount!.Id.Should().NotBeNull().And.Be(_chosenDiscountId);

        testVariant.Attributes.Should().NotBeNull().And.HaveCount(1);
        testVariant.Attributes[0].Id.Should().Be(_otherAttributeId);

        testVariant.VariantImages.Should().NotBeNull().And.HaveCount(1);
        testVariant.VariantImages[0].ImageId.Should().Be(_otherImageId);
        testVariant.VariantImages[0].VariantId.Should().Be(_chosenVariantId);

        testProduct!.Variants.Should().NotBeNull().And.HaveCount(2); //both should not be thumbnail variants after this
        testProduct!.Variants[0].IsThumbnailVariant.Should().BeFalse();
        testProduct!.Variants[1].IsThumbnailVariant.Should().BeFalse();
    }

    [Test, Order(240)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveAllImagesFromVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testGatewayUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testGatewayUpdateVariantRequestModel.Id = _chosenVariantId;
        testGatewayUpdateVariantRequestModel.ImagesIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testGatewayUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.VariantImages.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(250)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveDiscountFromVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.DiscountId = ""; //null would ignore it, but empty string means remove it

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant.Should().NotBeNull();
        testVariant!.Discount.Should().BeNull();
    }

    [Test, Order(260)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveAttributesFromVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.Attributes.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(270)]
    public async Task UpdateVariant_ShouldSucceedButNotAddAttributesAndImagesThatAreInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateVariantRequestModel testUpdateVariantRequestModel = new TestGatewayUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.DiscountId = "bogusDiscountId";
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { "bogusAttributeId1", "bogusAttributeId2" };
        testUpdateVariantRequestModel.ImagesIds = new List<string>() { "bogusImageId1", "bogusImageId2", "bogusImageId3" };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayVariant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.Discount.Should().BeNull(); //because we removed the discount before. If there was a discount here that would remain.
        testVariant!.Attributes.Should().NotBeNull().And.HaveCount(0);
        testVariant.VariantImages.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(280)]
    public async Task DeleteVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayVariant/{variantId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(290)]
    public async Task DeleteVariant_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusVariantId = "bogusVariantId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayVariant/{bogusVariantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(300)]
    public async Task DeleteVariant_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayVariant/{variantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(310)]
    public async Task DeleteVariant_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayVariant/{variantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(320)]
    public async Task DeleteVariant_ShouldSucceedAndDeleteVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayVariant/{variantId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayVariant/{variantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetVariants_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayVariant/amount/10/includeDeactivated/true");

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
