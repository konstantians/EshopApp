using EshopApp.TransactionLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace EshopApp.TransactionLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class RefundControllerTests
{
    private string _chosenApiKey;
    private string _chosenSessionId;
    private HttpClient httpClient;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        /*************** CheckOutSession ***************/
        /*TestCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.PaymentMethodType = "card";
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://example.com/success";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://example.com/cancel";
        testCreateCheckOutSessionRequestModel.CustomerEmail = "customer@example.com";
        testCreateCheckOutSessionRequestModel.CouponPercentage = 10;
        testCreateCheckOutSessionRequestModel.PaymentOptionName = "customer@example.com";
        testCreateCheckOutSessionRequestModel.PaymentOptionDescription = "customer@example.com";
        testCreateCheckOutSessionRequestModel.PaymentOptionCostInEuro = 3;
        testCreateCheckOutSessionRequestModel.ShippingOptionName = "ExpressShipping";
        testCreateCheckOutSessionRequestModel.ShippingOptionDescription = "Delivery within 1-2 business days.";
        testCreateCheckOutSessionRequestModel.ShippingOptionCostInEuro = 10;
        testCreateCheckOutSessionRequestModel.CreateTransactionOrderItemRequestModels = new List<TestCreateTransactionOrderItemRequestModel>()
        {
            new TestCreateTransactionOrderItemRequestModel()
            {
                Name = "MyProduct",
                Description = "best description ever",
                ImageUrl = "https://example.com/images/myproduct.jpg",
                Quantity = 2,
                FinalUnitAmountInEuro = 15
            }
        };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/checkOutSession", testCreateCheckOutSessionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenSessionId = JsonSerializer.Deserialize<TestCreateCheckOutSessionResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.CheckOutSessionId!;*/
    }

    [Test, Order(10)]
    public async Task IssueRefund_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestIssueRefundRequestModel testIssueRefundRequestModel = new TestIssueRefundRequestModel();
        testIssueRefundRequestModel.PaymentIntentId = "paymentIntentId";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/refund", testIssueRefundRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task IssueRefund_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestIssueRefundRequestModel testIssueRefundRequestModel = new TestIssueRefundRequestModel();
        testIssueRefundRequestModel.PaymentIntentId = "paymentIntentId";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/refund", testIssueRefundRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task IssueRefund_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestIssueRefundRequestModel testIssueRefundRequestModel = new TestIssueRefundRequestModel();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/refund", testIssueRefundRequestModel); //the property PaymentIntentId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfNoTransactionOrderItemWereProvided()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestIssueRefundRequestModel testIssueRefundRequestModel = new TestIssueRefundRequestModel();
        testIssueRefundRequestModel.PaymentIntentId = "bogusPaymentIntentId";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/refund", testIssueRefundRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("PaymentIntentNotFoundWithGivenId");
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task IssueRefund_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.PostAsJsonAsync("api/refund", new TestIssueRefundRequestModel());

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
