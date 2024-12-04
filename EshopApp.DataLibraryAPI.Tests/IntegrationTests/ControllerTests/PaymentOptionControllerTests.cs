using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.PaymentOptionModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class PaymentOptionControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenPaymentOptionId;
    private string? _otherPaymentOptionId;
    private string? _otherPaymentOptionName;
    private string? _otherPaymentOptionNameAlias;

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

        /*************** Other Payment Option ***************/
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "OtherPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "card";
        testCreatePaymentOptionRequestModel.ExtraCost = 4;

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestPaymentOption otherTestPaymentOption = JsonSerializer.Deserialize<TestPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        _otherPaymentOptionId = otherTestPaymentOption.Id;
        _otherPaymentOptionName = otherTestPaymentOption.Name;
        _otherPaymentOptionNameAlias = otherTestPaymentOption.NameAlias;
    }

    [Test, Order(10)]
    public async Task CreatePaymentOption_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreatePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel); //name property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = _otherPaymentOptionName;
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(45)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionNameAlias()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = _otherPaymentOptionNameAlias;
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityNameAlias");
    }

    [Test, Order(50)]
    public async Task CreatePaymentOption_ShouldSucceedAndCreatePaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.Description = "My payment option description";
        testCreatePaymentOptionRequestModel.ExtraCost = 0;
        testCreatePaymentOptionRequestModel.IsDeactivated = false;
        testCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testPaymentOption.Should().NotBeNull();
        testPaymentOption!.Id.Should().NotBeNull();
        testPaymentOption!.Name.Should().NotBeNull().And.Be(testCreatePaymentOptionRequestModel.Name);
        testPaymentOption!.NameAlias.Should().NotBeNull().And.Be(testCreatePaymentOptionRequestModel.NameAlias);
        testPaymentOption!.Description.Should().NotBeNull().And.Be(testCreatePaymentOptionRequestModel.Description);
        testPaymentOption!.ExtraCost.Should().NotBeNull().And.Be(testCreatePaymentOptionRequestModel.ExtraCost);
        testPaymentOption.IsDeactivated.Should().BeFalse();
        testPaymentOption.ExistsInOrder.Should().BeFalse();
        _chosenPaymentOptionId = testPaymentOption.Id;
    }

    [Test, Order(60)]
    public async Task GetPaymentOptions_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/paymentOption/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetPaymentOptions_ShouldSucceedAndReturnPaymentOptions()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/paymentOption/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestPaymentOption>? testPaymentOptions = JsonSerializer.Deserialize<List<TestPaymentOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testPaymentOptions.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(80)]
    public async Task GetPaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/paymentOption/{paymentOptionId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetPaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusPaymentOptionId = "bogusPaymentOptionId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/paymentOption/{bogusPaymentOptionId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetPaymentOption_ShouldSucceedAndReturnPaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/paymentOption/{paymentOptionId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testPaymentOption.Should().NotBeNull();
        testPaymentOption!.Id.Should().NotBeNull().And.Be(paymentOptionId);
        testPaymentOption!.Name.Should().NotBeNull();
        testPaymentOption!.NameAlias.Should().NotBeNull();
        testPaymentOption!.Description.Should().NotBeNull();
        testPaymentOption!.ExtraCost.Should().NotBeNull();
        testPaymentOption!.IsDeactivated.Should().BeFalse();
        testPaymentOption!.ExistsInOrder.Should().BeFalse();
    }

    [Test, Order(110)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.Id = "bogusPaymentOptionId";
        testUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testUpdatePaymentOptionRequestModel.Name = _otherPaymentOptionName;
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(145)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionNameAlias()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testUpdatePaymentOptionRequestModel.NameAlias = _otherPaymentOptionNameAlias;
        testUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityNameAlias");
    }

    [Test, Order(150)]
    public async Task UpdatePaymentOption_ShouldSucceedAndUpdatePaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdatePaymentOptionRequestModel testUpdatePaymentOptionRequestModel = new TestUpdatePaymentOptionRequestModel();
        testUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/paymentOption", testUpdatePaymentOptionRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/paymentOption/{_chosenPaymentOptionId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestPaymentOption>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testPaymentOption!.Name.Should().NotBeNull().And.Be(testUpdatePaymentOptionRequestModel.Name);
        testPaymentOption!.NameAlias.Should().NotBeNull().And.Be(testUpdatePaymentOptionRequestModel.NameAlias);
        testPaymentOption!.Description.Should().NotBeNull().And.Be(testUpdatePaymentOptionRequestModel.Description);
        testPaymentOption!.ExtraCost.Should().NotBeNull().And.Be(testUpdatePaymentOptionRequestModel.ExtraCost);
        testPaymentOption.IsDeactivated.Should().BeFalse();
        testPaymentOption.ExistsInOrder.Should().BeFalse();
        testPaymentOption.PaymentDetails.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(160)]
    public async Task DeletePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/paymentOption/{paymentOptionId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task DeletePaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusPaymentOptionId = "bogusPaymentOptionId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/paymentOption/{bogusPaymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task DeletePaymentOption_ShouldSucceedAndDeletePaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/paymentOption/{paymentOptionId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/paymentOption/{paymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetPaymentOptions_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/paymentOption/amount/10/includeDeactivated/true");

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
