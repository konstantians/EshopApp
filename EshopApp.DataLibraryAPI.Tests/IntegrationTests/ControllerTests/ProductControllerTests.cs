using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AttributeModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.CategoryModels;
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
internal class ProductControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenProductId;
    private string? _otherProductName;
    private string? _otherProductCode;
    private string? _firstCategoryId;
    private string? _secondCategoryId;
    private string? _thirdCategoryId;
    private string? _chosenAttributeId;
    private string? _chosenImageId;

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
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Coupons", "dbo.UserCoupons" },
            "Data Database Successfully Cleared!"
        );

        /*************** Other Product ***************/
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "OtherName";
        testCreateProductRequestModel.Code = "OtherCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        _otherProductName = testCreateProductRequestModel.Name;
        _otherProductCode = testCreateProductRequestModel.Code;

        /*************** Category 1 ***************/
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "Category1";
        response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _firstCategoryId = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Category 2 ***************/
        testCreateCategoryRequestModel.Name = "Category2";
        response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _secondCategoryId = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Category 3 ***************/
        testCreateCategoryRequestModel.Name = "Category3";
        response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _thirdCategoryId = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Attribute ***************/
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";
        response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenAttributeId = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Image ***************/
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "Image";
        testCreateImageRequestModel.ImagePath = "Path";
        response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenImageId = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

    }

    [Test, Order(10)]
    public async Task CreateProduct_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInProductEntity()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel); //The name value of the product is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInProductVariantEntity()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = -1; //This can not be negative for instance
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel); //The UnitsInStock value of the variant can not be negative and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(50)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfVariantEntityIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel); //The Variant Property is mising and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(60)]
    public async Task CreateProduct_ShouldSucceedAndCreateProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        testCreateProductRequestModel.CategoryIds = new List<string>() { _firstCategoryId!, _secondCategoryId! };

        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testProduct.Should().NotBeNull();
        testProduct!.Id.Should().NotBeNull();
        testProduct!.Name.Should().NotBeNull().And.Be(testCreateProductRequestModel.Name);
        testProduct!.Code.Should().NotBeNull().And.Be(testCreateProductRequestModel.Code);
        testProduct!.Description.Should().NotBeNull().And.Be(testCreateProductRequestModel.Description);
        testProduct!.Categories.Should().NotBeNull().And.HaveCount(2);
        testProduct!.Variants[0]!.SKU.Should().Be(testCreateVariantRequestModel.SKU);
        testProduct!.Variants[0]!.Price.Should().Be(testCreateVariantRequestModel.Price);
        testProduct!.Variants[0]!.UnitsInStock.Should().Be(testCreateVariantRequestModel.UnitsInStock);
        testProduct!.Variants[0]!.ProductId.Should().NotBeNull().And.Be(testProduct.Id);
        testProduct!.Variants[0]!.Attributes.Should().NotBeNull().And.HaveCount(1);
        testProduct!.Variants[0]!.VariantImages.Should().NotBeNull().And.HaveCount(1);
        _chosenProductId = testProduct.Id;
    }

    [Test, Order(70)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProduct";
        testCreateProductRequestModel.Code = "DifferentCode";
        testCreateProductRequestModel.Description = "My product description";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "DifferentSku";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(80)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductCode()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "DifferentProduct";
        testCreateProductRequestModel.Code = "MyCode";
        testCreateProductRequestModel.Description = "My product description";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "DifferentSku";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateProductCode");
    }

    [Test, Order(90)]
    public async Task CreateProduct_ShouldFailAndReturnBadRequest_IfDuplicateVariantSKU()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "DifferentProduct";
        testCreateProductRequestModel.Code = "DifferentCode";
        testCreateProductRequestModel.Description = "My product description";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MySKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.IsThumbnailVariant = false;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.AttributeIds = new List<string>() { _chosenAttributeId! };

        var testCreateVariantImageRequestModel = new TestCreateVariantImageRequestModel();
        testCreateVariantImageRequestModel.IsThumbNail = true;
        testCreateVariantImageRequestModel.ImageId = _chosenImageId;

        testCreateVariantRequestModel.VariantImageRequestModels.Add(testCreateVariantImageRequestModel);
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateVariantSku");
    }

    [Test, Order(100)]
    public async Task GetProducts_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/product/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetProducts_ShouldSucceedAndReturnProducts()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/product/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestProduct>? testProducts = JsonSerializer.Deserialize<List<TestProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testProducts.Should().NotBeNull().And.HaveCount(2); //one we created on the setup and one in the previous tests
    }

    [Test, Order(120)]
    public async Task GetProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/product/{productId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(130)]
    public async Task GetProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusProductId = "bogusProductId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/product/{bogusProductId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(140)]
    public async Task GetProduct_ShouldSucceedAndReturnProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/product/{productId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testProduct.Should().NotBeNull();
        testProduct!.Id.Should().NotBeNull().And.Be(productId);
        testProduct!.Name.Should().NotBeNull();
        testProduct.Code.Should().NotBeNull();
        testProduct.Description.Should().NotBeNull();
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(150)]
    public async Task UpdateProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = _chosenProductId;
        testUpdateProductRequestModel.Code = "MyCodeUpdated";
        testUpdateProductRequestModel.Name = "MyProductUpdated";
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testUpdateProductRequestModel.IsDeactivated = false; //false is the default value of the property, but I am leaving it here for visuals, if we want to deactivate the product then we need to set the flag to true

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(160)]
    public async Task UpdateProduct_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Code = "MyCodeUpdated";
        testUpdateProductRequestModel.Name = "MyProductUpdated";
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(170)]
    public async Task UpdateProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = "BogusProductId";
        testUpdateProductRequestModel.Code = "MyCodeUpdated";
        testUpdateProductRequestModel.Name = "MyProductUpdated";
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = _chosenProductId;
        testUpdateProductRequestModel.Code = "MyCodeUpdated";
        testUpdateProductRequestModel.Name = _otherProductName;
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateProduct_ShouldFailAndReturnBadRequest_IfDuplicateProductCode()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = _chosenProductId;
        testUpdateProductRequestModel.Code = _otherProductCode;
        testUpdateProductRequestModel.Name = "MyProductUpdated";
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };
        testUpdateProductRequestModel.IsDeactivated = false;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateProductCode");
    }

    [Test, Order(200)]
    public async Task UpdateProduct_ShouldSucceedAndUpdateProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = _chosenProductId;
        testUpdateProductRequestModel.Code = "MyCodeUpdated";
        testUpdateProductRequestModel.Name = "MyProductUpdated";
        testUpdateProductRequestModel.Description = "My description updated";
        testUpdateProductRequestModel.CategoryIds = new List<string>() { _thirdCategoryId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/product/{_chosenProductId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testProduct!.Code.Should().NotBeNull().And.Be("MyCodeUpdated");
        testProduct!.Name.Should().NotBeNull().And.Be("MyProductUpdated");
        testProduct!.Description.Should().NotBeNull().And.Be("My description updated");
        testProduct!.Categories.Should().NotBeNull().And.HaveCount(1);
        testProduct!.Categories[0].Id.Should().Be(_thirdCategoryId);
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(205)]
    public async Task UpdateProduct_ShouldSucceedAndRemoveVariantFromProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateProductRequestModel testUpdateProductRequestModel = new TestUpdateProductRequestModel();
        testUpdateProductRequestModel.Id = _chosenProductId;
        testUpdateProductRequestModel.VariantIds = new List<string>();

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/product", testUpdateProductRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/product/{_chosenProductId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestProduct? testProduct = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testProduct!.Code.Should().NotBeNull().And.Be("MyCodeUpdated");
        testProduct!.Name.Should().NotBeNull().And.Be("MyProductUpdated");
        testProduct!.Description.Should().NotBeNull().And.Be("My description updated");
        testProduct!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(210)]
    public async Task DeleteProduct_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/product/{productId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteProduct_ShouldFailAndReturnNotFound_IfProductNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusProductId = "bogusProductId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/product/{bogusProductId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(230)]
    public async Task DeleteProduct_ShouldSucceedAndDeleteProduct()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string productId = _chosenProductId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/product/{productId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/product/{productId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetProducts_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/product/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Coupons", "dbo.UserCoupons" },
            "Data Database Successfully Cleared!"
        );
    }
}
