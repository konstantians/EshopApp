using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.CategoryModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ProductModels;
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
internal class CategoryControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenCategoryId;
    private string? _chosenProductId;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
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
        testCreateVariantRequestModel.SKU = "ProductVariatSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateCategory_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCategory_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCategory_ShouldSucceedAndCreateCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCategory? testCategory = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCategory.Should().NotBeNull();
        testCategory!.Id.Should().NotBeNull();
        testCategory!.Name.Should().NotBeNull().And.Be(testCreateCategoryRequestModel.Name);
        _chosenCategoryId = testCategory.Id;
    }

    [Test, Order(50)]
    public async Task CreateCategory_ShouldFailAndReturnBadRequest_IfDuplicateCategoryName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "MyCategory";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(60)]
    public async Task GetCategories_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/category/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetCategories_ShouldSucceedAndReturnCategories()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/category/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestCategory>? testCategories = JsonSerializer.Deserialize<List<TestCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategories.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(80)]
    public async Task GetCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/category/{categoryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCategoryId = "bogusCategoryId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/category/{bogusCategoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetCategory_ShouldSucceedAndReturnCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/category/{categoryId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCategory? testCategory = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCategory.Should().NotBeNull();
        testCategory!.Id.Should().NotBeNull().And.Be(categoryId);
        testCategory!.Name.Should().NotBeNull();
    }

    [Test, Order(110)]
    public async Task UpdateCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateCategory_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Id = "bogusCategoryId";
        testUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateCategory_ShouldFailAndReturnBadRequest_IfDuplicateCategoryName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testUpdateCategoryRequestModel.Name = "AnotherCategory";
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        TestCreateCategoryRequestModel testCreateCategoryRequestModel = new TestCreateCategoryRequestModel();
        testCreateCategoryRequestModel.Name = "AnotherCategory";
        await httpClient.PostAsJsonAsync("api/category/", testCreateCategoryRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(150)]
    public async Task UpdateCategory_ShouldSucceedAndUpdateCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testUpdateCategoryRequestModel.Name = "MyCategoryUpdated";
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { _chosenProductId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/category/{_chosenCategoryId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestCategory? testCategory = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCategory!.Name.Should().NotBeNull().And.Be(testUpdateCategoryRequestModel.Name);
        testCategory.Products.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(155)]
    public async Task UpdateCategory_ShouldSucceedAndRemoveProductFromCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCategoryRequestModel testUpdateCategoryRequestModel = new TestUpdateCategoryRequestModel();
        testUpdateCategoryRequestModel.Id = _chosenCategoryId;
        testUpdateCategoryRequestModel.ProductIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/category", testUpdateCategoryRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/category/{_chosenCategoryId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestCategory? testCategory = JsonSerializer.Deserialize<TestCategory>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCategory!.Products.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(160)]
    public async Task DeleteCategory_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/category/{categoryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task DeleteCategory_ShouldFailAndReturnNotFound_IfCategoryNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCategoryId = "bogusCategoryId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/category/{bogusCategoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task DeleteCategory_ShouldSucceedAndDeleteCategory()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string categoryId = _chosenCategoryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/category/{categoryId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/category/{categoryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetCategories_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/category/amount/10");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );
    }
}
