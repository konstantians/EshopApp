using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AttributeModels;
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
internal class AttributeControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenAttributeId;
    private string? _chosenProductId;
    private string? _chosenVariantId;
    private string? _otherAttributeId;
    private string? _otherAttributeName;

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
        testCreateProductRequestModel.Name = "OtherName";
        testCreateProductRequestModel.Code = "OtherCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _chosenVariantId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;

        /*************** Other Attribute ***************/
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "OtherAttribute";
        response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherAttributeId = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherAttributeName = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateAttribute_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateAttribute_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel); //the property name is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateAttribute_ShouldFailAndReturnBadRequest_IfDuplicateAttributeName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = _otherAttributeName;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(50)]
    public async Task CreateAttribute_ShouldSucceedAndCreateAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateAttributeRequestModel testCreateAttributeRequestModel = new TestCreateAttributeRequestModel();
        testCreateAttributeRequestModel.Name = "MyAttribute";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/attribute", testCreateAttributeRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppAttribute? testAttribute = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testAttribute.Should().NotBeNull();
        testAttribute!.Id.Should().NotBeNull();
        testAttribute!.Name.Should().NotBeNull().And.Be(testCreateAttributeRequestModel.Name);
        _chosenAttributeId = testAttribute.Id;
    }

    [Test, Order(60)]
    public async Task GetAttributes_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/attribute/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetAttributes_ShouldSucceedAndReturnAttributes()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/attribute/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppAttribute>? testAttributes = JsonSerializer.Deserialize<List<TestAppAttribute>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAttributes.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(80)]
    public async Task GetAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/attribute/{attributeId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusAttributeId = "bogusAttributeId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/attribute/{bogusAttributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetAttribute_ShouldSucceedAndReturnAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/attribute/{attributeId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppAttribute? testAttribute = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testAttribute.Should().NotBeNull();
        testAttribute!.Id.Should().NotBeNull().And.Be(attributeId);
        testAttribute!.Name.Should().NotBeNull();
    }

    [Test, Order(110)]
    public async Task UpdateAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateAttribute_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Id = "bogusAttributeId";
        testUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateAttribute_ShouldFailAndReturnBadRequest_IfDuplicateAttributeName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testUpdateAttributeRequestModel.Name = _otherAttributeName;
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(150)]
    public async Task UpdateAttribute_ShouldSucceedAndUpdateAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testUpdateAttributeRequestModel.Name = "MyAttributeUpdated";
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { _chosenVariantId! };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/attribute/{_chosenAttributeId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestAppAttribute? testAttribute = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testAttribute!.Name.Should().NotBeNull().And.Be(testUpdateAttributeRequestModel.Name);
        testAttribute.Variants.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(155)]
    public async Task UpdateAttribute_ShouldSucceedAndRemoveVariantFromAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateAttributeRequestModel testUpdateAttributeRequestModel = new TestUpdateAttributeRequestModel();
        testUpdateAttributeRequestModel.Id = _chosenAttributeId;
        testUpdateAttributeRequestModel.VariantIds = new List<string>() { };

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/attribute", testUpdateAttributeRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/attribute/{_chosenAttributeId}");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestAppAttribute? testAttribute = JsonSerializer.Deserialize<TestAppAttribute>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testAttribute!.Variants.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(160)]
    public async Task DeleteAttribute_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/attribute/{attributeId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task DeleteAttribute_ShouldFailAndReturnNotFound_IfAttributeNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusAttributeId = "bogusAttributeId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/attribute/{bogusAttributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task DeleteAttribute_ShouldSucceedAndDeleteAttribute()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string attributeId = _chosenAttributeId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/attribute/{attributeId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/attribute/{attributeId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetAttributes_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/attribute/amount/10");

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
