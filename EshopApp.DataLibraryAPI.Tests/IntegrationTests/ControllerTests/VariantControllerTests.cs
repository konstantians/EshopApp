using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AttributeModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.DiscountModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ImageModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ProductModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantImageModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class VariantControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
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
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        /*************** Product & Variant ***************/
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherVariantId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;
        _otherVariantSku = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].SKU;

        /*************** Image ***************/
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "Image";
        testCreateImageRequestModel.ImagePath = "Path";
        response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenImageId = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other Image ***************/
        testCreateImageRequestModel.Name = "OtherImage";
        testCreateImageRequestModel.ImagePath = "OtherPath";
        response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherImageId = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Discount ***************/
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 10;
        response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenDiscountId = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Attribute ***************/
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";
        response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenAttributeId = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other Attribute ***************/
        testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "OtherAttribute";
        response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherAttributeId = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateVariant_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.ProductId = _chosenProductId;
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.ProductId = _chosenProductId;
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateVariant_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel); //here productId is missing, which is required, so the model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(30)]
    public async Task CreateVariant_ShouldFailAndReturnNotFound_IfInvalidProductId()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.ProductId = "bogusProductId";
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidProductIdWasGiven");
    }

    [Test, Order(30)]
    public async Task CreateVariant_ShouldFailAndReturnBadRequest_IfDuplicateVariantSKU()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.ProductId = _chosenProductId;
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(40)]
    public async Task CreateVariant_ShouldSucceedAndCreateVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.IsThumbnailVariant = true;
        testCreateVariantRequestModel.ProductId = _chosenProductId;
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateVariantRequestModel.AttributeIds.Add(_chosenAttributeId!);
        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;
        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage productGetResponse = await httpClient.GetAsync($"api/product/{_chosenProductId}/includeDeactivated/true");
        string? productResponseBody = await productGetResponse.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(productResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull();
        testVariant!.SKU.Should().NotBeNull().And.Be(testCreateVariantRequestModel.SKU);
        testVariant!.Price.Should().Be(testCreateVariantRequestModel.Price);
        testVariant!.UnitsInStock.Should().Be(testCreateVariantRequestModel.UnitsInStock);
        testVariant!.IsThumbnailVariant.Should().Be(testCreateVariantRequestModel.IsThumbnailVariant);

        testVariant!.Discount.Should().NotBeNull();
        testVariant!.Discount!.Id.Should().NotBeNull().And.Be(_chosenDiscountId);

        testVariant!.Attributes.Should().HaveCount(1);
        testVariant!.Attributes[0].Should().NotBeNull();
        testVariant!.Attributes[0].Id.Should().Be(_chosenAttributeId);

        testVariant!.VariantImages.Should().HaveCount(1);
        testVariant!.VariantImages[0].Should().NotBeNull();
        testVariant!.VariantImages[0].IsThumbNail.Should().Be(testCreateVariantImageRequestModel.IsThumbNail);
        testVariant!.VariantImages[0].ImageId.Should().NotBeNull().And.Be(_chosenImageId);

        testProduct!.Variants.Should().HaveCount(2);
        testProduct!.Variants.FirstOrDefault(variant => variant.Id == testVariant.Id)!.IsThumbnailVariant.Should()!.BeTrue();
        testProduct!.Variants.FirstOrDefault(variant => variant.Id == _otherVariantId)!.IsThumbnailVariant.Should()!.BeFalse();

        _chosenVariantId = testVariant.Id;
        _chosenVariantSku = testVariant.SKU;
    }

    [Test, Order(60)]
    public async Task GetVariants_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/variant/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetVariants_ShouldSucceedAndReturnVariants()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/variant/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestVariant>? testCategories = JsonSerializer.Deserialize<List<TestVariant>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategories.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(80)]
    public async Task GetVariantById_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/id/{variantId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetVariantById_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusVariantId = "bogusVariantId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/id/{bogusVariantId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetVariantById_ShouldSucceedAndReturnVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/id/{variantId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull().And.Be(variantId);
        testVariant!.SKU.Should().NotBeNull().And.Be(_chosenVariantSku);
        testVariant!.ProductId.Should().NotBeNull().And.Be(_chosenProductId);
    }

    [Test, Order(110)]
    public async Task GetVariantBySku_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string variantSku = _chosenVariantSku!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/sku/{_chosenVariantSku}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task GetVariantBySku_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusVariantSku = "bogusVariantSku";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/sku/{bogusVariantSku}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(130)]
    public async Task GetVariantBySku_ShouldSucceedAndReturnVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string variantSku = _chosenVariantSku!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/variant/sku/{variantSku}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testVariant.Should().NotBeNull();
        testVariant!.Id.Should().NotBeNull().And.Be(_chosenVariantId);
        testVariant!.SKU.Should().NotBeNull().And.Be(variantSku);
        testVariant!.ProductId.Should().NotBeNull().And.Be(_chosenProductId);
    }

    [Test, Order(140)]
    public async Task UpdateVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.SKU = "UpdatedSku";
        testUpdateVariantRequestModel.Price = 20;
        testUpdateVariantRequestModel.IsThumbnailVariant = false;
        testUpdateVariantRequestModel.UnitsInStock = 30;
        testUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task UpdateVariant_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.SKU = "UpdatedSku";
        testUpdateVariantRequestModel.Price = 20;
        testUpdateVariantRequestModel.IsThumbnailVariant = false;
        testUpdateVariantRequestModel.UnitsInStock = 30;
        testUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(160)]
    public async Task UpdateVariant_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = "BogusVariantId";
        testUpdateVariantRequestModel.SKU = "UpdatedSku";
        testUpdateVariantRequestModel.Price = 20;
        testUpdateVariantRequestModel.IsThumbnailVariant = false;
        testUpdateVariantRequestModel.UnitsInStock = 30;
        testUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(170)]
    public async Task UpdateVariant_ShouldFailAndReturnBadRequest_IfDuplicateVariantSku()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.SKU = "OtherSKU";
        testUpdateVariantRequestModel.Price = 20;
        testUpdateVariantRequestModel.IsThumbnailVariant = false;
        testUpdateVariantRequestModel.UnitsInStock = 30;
        testUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(180)]
    public async Task UpdateVariant_ShouldSucceedAndUpdateVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.SKU = "UpdatedSku";
        testUpdateVariantRequestModel.Price = 20;
        testUpdateVariantRequestModel.IsThumbnailVariant = false;
        testUpdateVariantRequestModel.UnitsInStock = 30;
        testUpdateVariantRequestModel.DiscountId = _chosenDiscountId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { _otherAttributeId! };
        testUpdateVariantRequestModel.ImagesIds = new() { _otherImageId! };
        testUpdateVariantRequestModel.ImageIdThatShouldBeThumbnail = _otherImageId!;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage productGetResponse = await httpClient.GetAsync($"api/product/{_chosenProductId}/includeDeactivated/true");
        string? productResponseBody = await productGetResponse.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(productResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.SKU.Should().NotBeNull().And.Be(testUpdateVariantRequestModel.SKU);
        testVariant!.Price.Should().Be(testUpdateVariantRequestModel.Price);
        testVariant.IsThumbnailVariant.Should().Be(testUpdateVariantRequestModel.IsThumbnailVariant);
        testVariant.UnitsInStock.Should().Be(testUpdateVariantRequestModel.UnitsInStock);

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

    [Test, Order(190)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveAllImagesFromVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.ImagesIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.VariantImages.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(195)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveDiscountFromVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.DiscountId = ""; //null would ignore it, but empty string means remove it

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant.Should().NotBeNull();
        testVariant!.Discount.Should().BeNull();
    }

    [Test, Order(197)]
    public async Task UpdateVariant_ShouldSucceedAndRemoveAttributesFromVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.Attributes.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(198)]
    public async Task UpdateVariant_ShouldSucceedButNotAddAttributesAndImagesThatAreInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.DiscountId = "bogusDiscountId";
        testUpdateVariantRequestModel.AttributeIds = new List<string>() { "bogusAttributeId1", "bogusAttributeId2" };
        testUpdateVariantRequestModel.ImagesIds = new List<string>() { "bogusImageId1", "bogusImageId2", "bogusImageId3" };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testVariant!.Discount.Should().BeNull(); //because we removed the discount before. If there was a discount here that would remain.
        testVariant.Attributes.Should().NotBeNull().And.HaveCount(0);
        testVariant.VariantImages.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(200)]
    public async Task DeleteVariant_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/variant/{variantId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(210)]
    public async Task DeleteVariant_ShouldFailAndReturnNotFound_IfVariantNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusVariantId = "bogusVariantId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/variant/{bogusVariantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(220)]
    public async Task DeleteVariant_ShouldSucceedAndDeleteVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string variantId = _chosenVariantId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/variant/{variantId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/variant/{variantId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetVariants_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/variant/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );
    }
}
