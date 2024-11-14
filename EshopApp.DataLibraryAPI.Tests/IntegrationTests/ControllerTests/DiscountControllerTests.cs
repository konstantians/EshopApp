using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.DiscountModels;
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
internal class DiscountControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenDiscountId;
    private string? _otherDiscountId;
    private string? _otherDiscountName;
    private string? _chosenProductId;
    private string? _chosenVariantId;

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
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages" },
            "Data Database Successfully Cleared!"
        );

        /*************** Product & Variant ***************/
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "VariantSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _chosenVariantId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;

        /*************** Other Discount ***************/
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "OtherDiscount";
        testCreateDiscountRequestModel.Percentage = 10;
        response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherDiscountId = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherDiscountName = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateDiscount_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateDiscount_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel); //here name is missing, which makes the request model invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateDiscount_ShouldFailAndReturnBadRequest_IfDuplicateDiscountName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = _otherDiscountName;
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(50)]
    public async Task CreateDiscount_ShouldSucceedAndCreateDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestDiscount? testDiscount = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testDiscount.Should().NotBeNull();
        testDiscount!.Id.Should().NotBeNull();
        testDiscount!.Name.Should().NotBeNull().And.Be(testCreateDiscountRequestModel.Name);
        testDiscount!.Percentage.Should().Be(testCreateDiscountRequestModel.Percentage);
        _chosenDiscountId = testDiscount.Id;
    }

    [Test, Order(60)]
    public async Task GetDiscounts_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/discount/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetDiscounts_ShouldSucceedAndReturnDiscounts()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/discount/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestDiscount>? testDiscounts = JsonSerializer.Deserialize<List<TestDiscount>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testDiscounts.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(80)]
    public async Task GetDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/discount/{discountId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusDiscountId = "bogusDiscountId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/discount/{bogusDiscountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetDiscount_ShouldSucceedAndReturnDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/discount/{discountId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestDiscount? testDiscount = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testDiscount.Should().NotBeNull();
        testDiscount!.Id.Should().NotBeNull().And.Be(discountId);
        testDiscount!.Name.Should().NotBeNull();
    }

    [Test, Order(110)]
    public async Task UpdateDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testUpdateDiscountRequestModel.Percentage = 50;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateDiscount_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testUpdateDiscountRequestModel.Percentage = 50;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Id = "bogusDiscountId";
        testUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testUpdateDiscountRequestModel.Percentage = 50;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateDiscount_ShouldFailAndReturnBadRequest_IfDuplicateDiscountName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testUpdateDiscountRequestModel.Name = _otherDiscountName;
        testUpdateDiscountRequestModel.Percentage = 50;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(150)]
    public async Task UpdateDiscount_ShouldSucceedAndUpdateDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testUpdateDiscountRequestModel.Name = "MyDiscountUpdated";
        testUpdateDiscountRequestModel.Percentage = 50;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/discount/{_chosenDiscountId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestDiscount? testDiscount = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testDiscount!.Name.Should().NotBeNull().And.Be(testUpdateDiscountRequestModel.Name);
        testDiscount!.Percentage.Should().Be(testUpdateDiscountRequestModel.Percentage);
        testDiscount.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(155)]
    public async Task UpdateDiscount_ShouldSucceedAndRemoveVariantFromDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateDiscountRequestModel testUpdateDiscountRequestModel = new TestUpdateDiscountRequestModel();
        testUpdateDiscountRequestModel.Id = _chosenDiscountId;
        testUpdateDiscountRequestModel.VariantIds = new List<string>() { };
        testUpdateDiscountRequestModel.Percentage = 50;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/discount", testUpdateDiscountRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/discount/{_chosenDiscountId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestDiscount? testDiscount = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testDiscount!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(160)]
    public async Task DeleteDiscount_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/discount/{discountId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task DeleteDiscount_ShouldFailAndReturnNotFound_IfDiscountNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusDiscountId = "bogusDiscountId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/discount/{bogusDiscountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task DeleteDiscount_ShouldSucceedAndDeleteDiscount()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string discountId = _chosenDiscountId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/discount/{discountId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/discount/{discountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetDiscounts_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/discount/amount/10");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages" },
            "Data Database Successfully Cleared!"
        );
    }
}
