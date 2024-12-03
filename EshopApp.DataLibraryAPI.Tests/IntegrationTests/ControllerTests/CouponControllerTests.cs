using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CouponModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class CouponControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenUserId;
    private string? _chosenUniversalCouponId;
    private string? _chosenUserSpecificCouponId;
    private string? _otherCouponId;
    private string? _otherCouponCode;
    private string? _chosenUniversalUserCouponId;
    private string? _chosenUserSpecificUserCouponId;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";
        _chosenUserId = "chosenUserId";

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions" },
            "Data Database Successfully Cleared!"
        );

        /*************** Other Coupon ***************/
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My other coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 10;
        testCreateCouponRequestModel.UsageLimit = 1;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _otherCouponId = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;
        _otherCouponCode = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Code!;
    }

    [Test, Order(10)]
    public async Task CreateCoupon_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 20;
        testCreateCouponRequestModel.UsageLimit = 2;
        testCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 20;
        testCreateCouponRequestModel.UsageLimit = 2;
        testCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCoupon_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInCouponEntity()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My coupon description";
        testCreateCouponRequestModel.UsageLimit = 2;
        testCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel); //The discount percentage value of the discount is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCoupon_ShouldSucceedAndCreateUniversalCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 20;
        testCreateCouponRequestModel.UsageLimit = 2;
        testCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCoupon.Should().NotBeNull();
        testCoupon!.Id.Should().NotBeNull();
        testCoupon!.Code.Should().NotBeNull().And.NotBe(testCreateCouponRequestModel.Code);
        testCoupon!.Description.Should().NotBeNull().And.Be(testCreateCouponRequestModel.Description);
        testCoupon!.DiscountPercentage.Should().NotBeNull().And.Be(testCreateCouponRequestModel.DiscountPercentage);
        testCoupon!.UsageLimit.Should().NotBeNull().And.Be(testCreateCouponRequestModel.UsageLimit);
        testCoupon!.DefaultDateIntervalInDays.Should().BeNull();
        testCoupon!.IsUserSpecific.Should().NotBeNull().And.Be(testCreateCouponRequestModel.IsUserSpecific);
        testCoupon!.IsDeactivated.Should().NotBeNull().And.Be(testCreateCouponRequestModel.IsDeactivated);
        testCoupon!.TriggerEvent.Should().NotBeNull().And.Be("NoTrigger");
        testCoupon!.StartDate.Should().NotBeNull().And.Be(testCreateCouponRequestModel.StartDate);
        testCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testCreateCouponRequestModel.ExpirationDate);
        _chosenUniversalCouponId = testCoupon.Id!;
    }

    [Test, Order(50)]
    public async Task CreateCoupon_ShouldSucceedAndCreateUserSpecificCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Description = "My user specific coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 40;
        testCreateCouponRequestModel.UsageLimit = 4;
        testCreateCouponRequestModel.DefaultDateIntervalInDays = 4;
        testCreateCouponRequestModel.IsUserSpecific = true; //it is user specific
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //Now this trigger event is applied correctly
        testCreateCouponRequestModel.StartDate = DateTime.Now; //this should become null, because the coupon is user specific
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2); //this should become null, because the coupon is user specific

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCoupon.Should().NotBeNull();
        testCoupon!.Id.Should().NotBeNull();
        testCoupon!.Code.Should().NotBeNull().And.NotBe(testCreateCouponRequestModel.Code);
        testCoupon!.Description.Should().NotBeNull().And.Be(testCreateCouponRequestModel.Description);
        testCoupon!.DiscountPercentage.Should().NotBeNull().And.Be(testCreateCouponRequestModel.DiscountPercentage);
        testCoupon!.UsageLimit.Should().NotBeNull().And.Be(testCreateCouponRequestModel.UsageLimit);
        testCoupon!.DefaultDateIntervalInDays.Should().NotBeNull().And.Be(testCreateCouponRequestModel.DefaultDateIntervalInDays);
        testCoupon!.IsUserSpecific.Should().NotBeNull().And.Be(testCreateCouponRequestModel.IsUserSpecific);
        testCoupon!.IsDeactivated.Should().NotBeNull().And.Be(testCreateCouponRequestModel.IsDeactivated);
        testCoupon!.TriggerEvent.Should().NotBeNull().And.Be("OnSignUp");
        testCoupon!.StartDate.Should().BeNull();
        testCoupon!.ExpirationDate.Should().BeNull();
        _chosenUserSpecificCouponId = testCoupon.Id!;
    }

    [Test, Order(60)]
    public async Task GetCoupons_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/coupon/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetCoupons_ShouldSucceedAndReturnCoupons()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/coupon/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestCoupon>? testCoupons = JsonSerializer.Deserialize<List<TestCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCoupons.Should().NotBeNull().And.HaveCount(3); //one we created on the setup and one in the previous tests
    }

    [Test, Order(80)]
    public async Task GetCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string? couponId = _chosenUniversalCouponId;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/coupon/{couponId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/coupon/{bogusCouponId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetCoupon_ShouldSucceedAndReturnCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/coupon/{couponId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCoupon.Should().NotBeNull();
        testCoupon!.Id.Should().NotBeNull().And.Be(couponId);
        testCoupon.Code.Should().NotBeNull();
        testCoupon.Description.Should().NotBeNull();
        testCoupon.DiscountPercentage.Should().NotBeNull();
        testCoupon.UsageLimit.Should().NotBeNull();
        testCoupon.DefaultDateIntervalInDays.Should().BeNull();
        testCoupon.IsUserSpecific.Should().NotBeNull().And.Be(false);
        testCoupon.IsDeactivated.Should().NotBeNull();
        testCoupon.TriggerEvent.Should().NotBeNull().And.Be("NoTrigger");
        testCoupon.StartDate.Should().NotBeNull();
        testCoupon.ExpirationDate.Should().NotBeNull();
    }

    [Test, Order(110)]
    public async Task UpdateCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateCoupon_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = "bogusCouponId";
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateCoupon_ShouldFailAndReturnBadRequest_IfDuplicateCouponCode()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = _otherCouponCode;
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityCode");
    }

    [Test, Order(150)]
    public async Task UpdateCoupon_ShouldSucceedAndUpdateUniversalCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 3; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{_chosenUniversalCouponId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.Code.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.Code);
        testCoupon!.Description.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.Description);
        testCoupon!.DiscountPercentage.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.DiscountPercentage);
        testCoupon!.UsageLimit.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.UsageLimit);
        testCoupon!.DefaultDateIntervalInDays.Should().BeNull();
        testCoupon!.TriggerEvent.Should().NotBeNull().And.Be("NoTrigger");
        testCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.ExpirationDate);
    }

    [Test, Order(150)]
    public async Task UpdateCoupon_ShouldSucceedAndUpdateUserSpecificCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCouponRequestModel testUpdateCouponRequestModel = new TestUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUserSpecificCouponId;
        testUpdateCouponRequestModel.Code = "MyNewUserSpecificCouponCode";
        testUpdateCouponRequestModel.Description = "My user specific coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 50;
        testUpdateCouponRequestModel.UsageLimit = 5;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5;
        testUpdateCouponRequestModel.TriggerEvent = "OnFirstOrder";
        testUpdateCouponRequestModel.StartDate = DateTime.Now.AddDays(1); //this should be ignored because the event is user specific
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3); //this should be ignored because the event is user specific

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon", testUpdateCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{_chosenUserSpecificCouponId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.Code.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.Code);
        testCoupon!.Description.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.Description);
        testCoupon!.DiscountPercentage.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.DiscountPercentage);
        testCoupon!.UsageLimit.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.UsageLimit);
        testCoupon!.DefaultDateIntervalInDays.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.DefaultDateIntervalInDays);
        testCoupon!.TriggerEvent.Should().NotBeNull().And.Be(testUpdateCouponRequestModel.TriggerEvent);
        testCoupon!.StartDate.Should().BeNull();
        testCoupon!.ExpirationDate.Should().BeNull();
    }

    [Test, Order(160)]
    public async Task AddCouponToUser_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(170)]
    public async Task AddCouponToUser_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        //the code can be autogenerated so I do not need to set it here and timesused, because it is null will become 0
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(180)]
    public async Task AddCouponToUser_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInUserCouponEntity()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel); //The userId of the user coupon is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(190)]
    public async Task AddCouponToUser_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = "bogusCouponId";
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(200)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUniversalCouponToUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{testAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull().And.NotBe(testAddCouponToUserRequestModel.Code);
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.StartDate.Should().NotBeNull().And.NotBe(testAddCouponToUserRequestModel.StartDate);
        testUserCoupon!.ExpirationDate.Should().NotBeNull().And.NotBe(testAddCouponToUserRequestModel.ExpirationDate);
        testUserCoupon!.UserId = testAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testAddCouponToUserRequestModel.CouponId;
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);
        _chosenUniversalUserCouponId = testUserCoupon!.Id;
    }

    [Test, Order(210)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUserSpecificCouponToUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenUserSpecificCouponId;
        //since the coupon is user specific the dates will be created using the date interval property of the coupon

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{testAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull();
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.UserId = testAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testAddCouponToUserRequestModel.CouponId;
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);
        _chosenUserSpecificUserCouponId = testUserCoupon!.Id;
    }

    [Test, Order(220)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUserSpecificCouponToUserWithCustomDatesAndCode()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.Code = "MyUserCouponCode";
        testAddCouponToUserRequestModel.StartDate = DateTime.Now.AddMonths(1);
        testAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenUserSpecificCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{testAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull();
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.UserId = testAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testAddCouponToUserRequestModel.CouponId;
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(2);
        TestUserCoupon? currentlyAddedUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Code == testAddCouponToUserRequestModel.Code);
        currentlyAddedUserCoupon!.StartDate.Should().NotBeNull().And.Be(testAddCouponToUserRequestModel.StartDate);
        currentlyAddedUserCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testAddCouponToUserRequestModel.ExpirationDate);
    }

    [Test, Order(230)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon/updateUserCoupon", testUpdateUserCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(240)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon/updateUserCoupon", testUpdateUserCouponRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(250)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnNotFound_IfUserCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Id = "bogusUserCouponId";
        testUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon/updateUserCoupon", testUpdateUserCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(260)]
    public async Task UpdateUserCoupon_ShouldSucceedAndUpdateUniversalUserCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon/updateUserCoupon", testUpdateUserCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{_chosenUniversalCouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);
        testCoupon!.UserCoupons[0].Code.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.Code);
        testCoupon!.UserCoupons[0].TimesUsed.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.TimesUsed);
        testCoupon!.UserCoupons[0].StartDate.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.StartDate);
        testCoupon!.UserCoupons[0].ExpirationDate.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.ExpirationDate);
    }

    [Test, Order(270)]
    public async Task UpdateUserCoupon_ShouldSucceedAndUpdateUserSpecificUserCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        //when it comes to user specific user coupons all the properties can be adjusted, so nothing will be overriden below
        TestUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Id = _chosenUserSpecificUserCouponId;
        testUpdateUserCouponRequestModel.Code = "MyNewUserSpecificUserCouponCode";
        testUpdateUserCouponRequestModel.TimesUsed = 1;
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(4);
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(5);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/coupon/updateUserCoupon", testUpdateUserCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/coupon/{_chosenUserSpecificCouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(2);
        TestUserCoupon? currentlyUpdatedUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Code == testUpdateUserCouponRequestModel.Code);
        currentlyUpdatedUserCoupon!.Code.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.Code);
        currentlyUpdatedUserCoupon!.TimesUsed.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.TimesUsed);
        currentlyUpdatedUserCoupon!.StartDate.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.StartDate);
        currentlyUpdatedUserCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.ExpirationDate);
    }

    [Test, Order(280)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/removeCouponFromUser/userCouponId/{userCouponId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(290)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnNotFound_IfUserCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusUserCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/removeCouponFromUser/userCouponId/{bogusUserCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(300)]
    public async Task RemoveCouponFromUser_ShouldSucceedAndRemoveCouponFromUser()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/removeCouponFromUser/userCouponId/{userCouponId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/coupon/removeCouponFromUser/userCouponId/{userCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(310)]
    public async Task DeleteCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/{couponId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(320)]
    public async Task DeleteCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/{bogusCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(330)]
    public async Task DeleteCoupon_ShouldSucceedAndDeleteCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/coupon/{couponId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/coupon/{couponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(400)]
    public async Task GetCoupons_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/coupon/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public void OnTimeTearDown()
    {
        httpClient.Dispose();
        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlAuthDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions" },
            "Data Database Successfully Cleared!"
        );
    }
}
