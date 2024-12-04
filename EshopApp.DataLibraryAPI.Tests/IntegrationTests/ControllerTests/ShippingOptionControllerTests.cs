using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.ShippingOptionModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class ShippingOptionControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenShippingOptionId;
    private string? _otherShippingOptionId;
    private string? _otherShippingOptionName;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        /*************** Other Shipping Option ***************/
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "OtherShippingOption";
        testCreateShippingOptionRequestModel.ExtraCost = 5;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _otherShippingOptionId = JsonSerializer.Deserialize<TestShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherShippingOptionName = JsonSerializer.Deserialize<TestShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateShippingOption_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testCreateShippingOptionRequestModel.Description = "My shipping option description";
        testCreateShippingOptionRequestModel.ExtraCost = 3;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        testCreateShippingOptionRequestModel.IsDeactivated = false;
        testCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testCreateShippingOptionRequestModel.Description = "My shipping option description";
        testCreateShippingOptionRequestModel.ExtraCost = 3;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        testCreateShippingOptionRequestModel.IsDeactivated = false;
        testCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateShippingOption_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Description = "My shipping option description";
        testCreateShippingOptionRequestModel.ExtraCost = 3;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        testCreateShippingOptionRequestModel.IsDeactivated = false;
        testCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel); //name property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateShippingOption_ShouldFailAndReturnBadRequest_IfDuplicateShippingOptionName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = _otherShippingOptionName;
        testCreateShippingOptionRequestModel.Description = "My shipping option description";
        testCreateShippingOptionRequestModel.ExtraCost = 3;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        testCreateShippingOptionRequestModel.IsDeactivated = false;
        testCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(50)]
    public async Task CreateShippingOption_ShouldSucceedAndCreateShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testCreateShippingOptionRequestModel.Description = "My shipping option description";
        testCreateShippingOptionRequestModel.ExtraCost = 3;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        testCreateShippingOptionRequestModel.IsDeactivated = false;
        testCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestShippingOption? testShippingOption = JsonSerializer.Deserialize<TestShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testShippingOption.Should().NotBeNull();
        testShippingOption!.Id.Should().NotBeNull();
        testShippingOption!.Name.Should().NotBeNull().And.Be(testCreateShippingOptionRequestModel.Name);
        testShippingOption!.Description.Should().NotBeNull().And.Be(testCreateShippingOptionRequestModel.Description);
        testShippingOption!.ExtraCost.Should().NotBeNull().And.Be(testCreateShippingOptionRequestModel.ExtraCost);
        testShippingOption.ContainsDelivery.Should().BeTrue();
        testShippingOption.IsDeactivated.Should().BeFalse();
        testShippingOption.ExistsInOrder.Should().BeFalse();
        _chosenShippingOptionId = testShippingOption.Id;
    }

    [Test, Order(60)]
    public async Task GetShippingOptions_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/shippingOption/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetShippingOptions_ShouldSucceedAndReturnShippingOptions()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/shippingOption/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestShippingOption>? testShippingOptions = JsonSerializer.Deserialize<List<TestShippingOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testShippingOptions.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(80)]
    public async Task GetShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/shippingOption/{shippingOptionId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusShippingOptionId = "bogusShippingOptionId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/shippingOption/{bogusShippingOptionId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetShippingOption_ShouldSucceedAndReturnShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/shippingOption/{shippingOptionId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestShippingOption? testShippingOption = JsonSerializer.Deserialize<TestShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testShippingOption.Should().NotBeNull();
        testShippingOption!.Id.Should().NotBeNull().And.Be(shippingOptionId);
        testShippingOption!.Name.Should().NotBeNull();
        testShippingOption!.Description.Should().NotBeNull();
        testShippingOption!.ExtraCost.Should().NotBeNull();
        testShippingOption!.ContainsDelivery.Should().BeTrue();
        testShippingOption!.IsDeactivated.Should().BeFalse();
        testShippingOption!.ExistsInOrder.Should().BeFalse();
    }

    [Test, Order(110)]
    public async Task UpdateShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateShippingOptionRequestModel testUpdateShippingOptionRequestModel = new TestUpdateShippingOptionRequestModel();
        testUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/shippingOption", testUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateShippingOption_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateShippingOptionRequestModel testUpdateShippingOptionRequestModel = new TestUpdateShippingOptionRequestModel();
        testUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/shippingOption", testUpdateShippingOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateShippingOptionRequestModel testUpdateShippingOptionRequestModel = new TestUpdateShippingOptionRequestModel();
        testUpdateShippingOptionRequestModel.Id = "bogusShippingOptionId";
        testUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/shippingOption", testUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateShippingOption_ShouldFailAndReturnBadRequest_IfDuplicateShippingOptionName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateShippingOptionRequestModel testUpdateShippingOptionRequestModel = new TestUpdateShippingOptionRequestModel();
        testUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testUpdateShippingOptionRequestModel.Name = _otherShippingOptionName;
        testUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/shippingOption", testUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(150)]
    public async Task UpdateShippingOption_ShouldSucceedAndUpdateShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateShippingOptionRequestModel testUpdateShippingOptionRequestModel = new TestUpdateShippingOptionRequestModel();
        testUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/shippingOption", testUpdateShippingOptionRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/shippingOption/{_chosenShippingOptionId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestShippingOption? testShippingOption = JsonSerializer.Deserialize<TestShippingOption>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testShippingOption!.Name.Should().NotBeNull().And.Be(testUpdateShippingOptionRequestModel.Name);
        testShippingOption!.Description.Should().NotBeNull().And.Be(testUpdateShippingOptionRequestModel.Description);
        testShippingOption!.ExtraCost.Should().NotBeNull().And.Be(testUpdateShippingOptionRequestModel.ExtraCost);
        testShippingOption.ContainsDelivery.Should().BeTrue();
        testShippingOption.IsDeactivated.Should().BeFalse();
        testShippingOption.ExistsInOrder.Should().BeFalse();
        testShippingOption.Orders.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(160)]
    public async Task DeleteShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/shippingOption/{shippingOptionId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task DeleteShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusShippingOptionId = "bogusShippingOptionId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/shippingOption/{bogusShippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task DeleteShippingOption_ShouldSucceedAndDeleteShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/shippingOption/{shippingOptionId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/shippingOption/{shippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetShippingOptions_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/shippingOption/amount/10/includeDeactivated/true");

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
