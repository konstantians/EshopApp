using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests;

internal class GatewayCouponControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 6000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _userId;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenUniversalCouponId;
    private string? _chosenUserSpecificCouponId;
    private string? _otherCouponId;
    private string? _otherCouponCode;
    private string? _chosenUniversalUserCouponId;
    private string? _chosenUserSpecificUserCouponId;

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

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        string? responseBody = await response.Content.ReadAsStringAsync();
        _userId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //set the headers for the below operations
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        /*************** Other Coupon ***************/
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestGatewayCreateCouponRequestModel testCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My other coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 10;
        testCreateCouponRequestModel.UsageLimit = 1;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testCreateCouponRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherCouponId = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;
        _otherCouponCode = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Code!;
    }

    [Test, Order(10)]
    public async Task CreateCoupon_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.DiscountPercentage = 20;
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.DiscountPercentage = 20;
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCoupon_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInCouponEntity()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel); //The discount percentage value of the discount is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCoupon_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.DiscountPercentage = 20;
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel); //The discount percentage value of the discount is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateCoupon_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.DiscountPercentage = 20;
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel); //The discount percentage value of the discount is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateCoupon_ShouldSucceedAndCreateUniversalCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCouponRequestModel testGatewayCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testGatewayCreateCouponRequestModel.Code = _otherCouponCode; //because this code already exists it should create a random different code
        testGatewayCreateCouponRequestModel.Description = "My coupon description";
        testGatewayCreateCouponRequestModel.DiscountPercentage = 20;
        testGatewayCreateCouponRequestModel.UsageLimit = 2;
        testGatewayCreateCouponRequestModel.DefaultDateIntervalInDays = 2;
        testGatewayCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testGatewayCreateCouponRequestModel.IsDeactivated = false;
        testGatewayCreateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should become NoTrigger since the event is universal and thus triggered based on date
        testGatewayCreateCouponRequestModel.StartDate = DateTime.Now;
        testGatewayCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testGatewayCreateCouponRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCoupon.Should().NotBeNull();
        testCoupon!.Id.Should().NotBeNull();
        testCoupon!.Code.Should().NotBeNull().And.NotBe(testGatewayCreateCouponRequestModel.Code);
        testCoupon!.Description.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.Description);
        testCoupon!.DiscountPercentage.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.DiscountPercentage);
        testCoupon!.UsageLimit.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.UsageLimit);
        testCoupon!.DefaultDateIntervalInDays.Should().BeNull();
        testCoupon!.IsUserSpecific.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.IsUserSpecific);
        testCoupon!.IsDeactivated.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.IsDeactivated);
        testCoupon!.TriggerEvent.Should().NotBeNull().And.Be("NoTrigger");
        testCoupon!.StartDate.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.StartDate);
        testCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testGatewayCreateCouponRequestModel.ExpirationDate);
        _chosenUniversalCouponId = testCoupon.Id!;
    }

    [Test, Order(70)]
    public async Task CreateCoupon_ShouldSucceedAndCreateUserSpecificCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCouponRequestModel testCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
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
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testCreateCouponRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(80)]
    public async Task GetCoupons_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCoupon/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetCoupons_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCoupon/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(100)]
    public async Task GetCoupons_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCoupon/amount/10/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(110)]
    public async Task GetCoupons_ShouldSucceedAndReturnCoupons()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayCoupon/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayCoupon>? testCoupons = JsonSerializer.Deserialize<List<TestGatewayCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCoupons.Should().NotBeNull().And.HaveCount(3); //one we created on the setup and one in the previous tests
    }

    [Test, Order(120)]
    public async Task GetCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string? couponId = _chosenUniversalCouponId;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCoupon/{couponId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(130)]
    public async Task GetCoupon_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string? couponId = _chosenUniversalCouponId;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCoupon/{couponId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(140)]
    public async Task GetCoupon_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string? couponId = _chosenUniversalCouponId;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCoupon/{couponId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(150)]
    public async Task GetCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCoupon/{bogusCouponId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(160)]
    public async Task GetCoupon_ShouldSucceedAndReturnCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayCoupon/{couponId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(170)]
    public async Task UpdateCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(180)]
    public async Task UpdateCoupon_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(190)]
    public async Task UpdateCoupon_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(200)]
    public async Task UpdateCoupon_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(210)]
    public async Task UpdateCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = "bogusCouponId";
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(220)]
    public async Task UpdateCoupon_ShouldFailAndReturnBadRequest_IfDuplicateCouponCode()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = _otherCouponCode;
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityCode");
    }

    [Test, Order(230)]
    public async Task UpdateCoupon_ShouldSucceedAndUpdateUniversalCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
        testUpdateCouponRequestModel.Id = _chosenUniversalCouponId;
        testUpdateCouponRequestModel.Code = "MyNewCouponCode";
        testUpdateCouponRequestModel.Description = "My coupon description updated";
        testUpdateCouponRequestModel.DiscountPercentage = 30;
        testUpdateCouponRequestModel.UsageLimit = 3;
        testUpdateCouponRequestModel.DefaultDateIntervalInDays = 5; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.TriggerEvent = "OnSignUp"; //this should be ignored because the event is universal
        testUpdateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{_chosenUniversalCouponId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(240)]
    public async Task UpdateCoupon_ShouldSucceedAndUpdateUserSpecificCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateCouponRequestModel testUpdateCouponRequestModel = new TestGatewayUpdateCouponRequestModel();
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
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon", testUpdateCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{_chosenUserSpecificCouponId}/includeDeactivated/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(250)]
    public async Task AddCouponToUser_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        //the code can be autogenerated so I do not need to set it here, and timesused because it is null will become 0
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(260)]
    public async Task AddCouponToUser_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInUserCouponEntity()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel); //The userId of the user coupon is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(270)]
    public async Task AddCouponToUser_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel); //The userId of the user coupon is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(280)]
    public async Task AddCouponToUser_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel); //The userId of the user coupon is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(290)]
    public async Task AddCouponToUser_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = "bogusCouponId";
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(3);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(300)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUniversalCouponToUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCode"; //since the coupon is universal the code will be the same as the code of the coupon(the code that I am giving here will be overriden)
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUniversalCouponId;
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddDays(1); //since the coupon is universal the start dates and expiration dates will be overriden
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddDays(50);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{testGatewayAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage getUserByIdTokenResponse = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{_userId}");
        string? getUserByIdResponseBody = await getUserByIdTokenResponse.Content.ReadAsStringAsync();
        GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(getUserByIdResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull().And.NotBe(testGatewayAddCouponToUserRequestModel.Code);
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.StartDate.Should().NotBeNull().And.NotBe(testGatewayAddCouponToUserRequestModel.StartDate);
        testUserCoupon!.ExpirationDate.Should().NotBeNull().And.NotBe(testGatewayAddCouponToUserRequestModel.ExpirationDate);
        testUserCoupon!.UserId = testGatewayAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testGatewayAddCouponToUserRequestModel.CouponId;

        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);

        getUserByIdTokenResponse.Should().NotBeNull();
        appUser.Should().NotBeNull();
        appUser!.UserCoupons.Should().HaveCount(1);
        appUser!.UserCoupons[0].CouponId.Should().Be(testGatewayAddCouponToUserRequestModel.CouponId);

        _chosenUniversalUserCouponId = testUserCoupon!.Id;
    }

    [Test, Order(310)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUserSpecificCouponToUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUserSpecificCouponId;
        //since the coupon is user specific the dates will be created using the date interval property of the coupon

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{testGatewayAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage getUserByIdTokenResponse = await httpClient.GetAsync($"api/gatewayAdmin/getuserbyid/{_userId}");
        string? getUserByIdResponseBody = await getUserByIdTokenResponse.Content.ReadAsStringAsync();
        GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(getUserByIdResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull();
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.UserId = testGatewayAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testGatewayAddCouponToUserRequestModel.CouponId;
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);

        getUserByIdTokenResponse.Should().NotBeNull();
        appUser.Should().NotBeNull();
        appUser!.UserCoupons.Should().HaveCount(2);

        _chosenUserSpecificUserCouponId = testUserCoupon!.Id;
    }

    [Test, Order(320)]
    public async Task AddCouponToUser_ShouldSucceedAndAddUserSpecificCouponToUserWithCustomDatesAndCode()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.Code = "MyUserCouponCode";
        testGatewayAddCouponToUserRequestModel.StartDate = DateTime.Now.AddMonths(1);
        testGatewayAddCouponToUserRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenUserSpecificCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayUserCoupon? testUserCoupon = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{testGatewayAddCouponToUserRequestModel.CouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testUserCoupon.Should().NotBeNull();
        testUserCoupon!.Id.Should().NotBeNull();
        testUserCoupon!.Code.Should().NotBeNull();
        testUserCoupon!.TimesUsed.Should().Be(0);
        testUserCoupon!.IsDeactivated.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.ExistsInOrder.Should().NotBeNull().And.BeFalse();
        testUserCoupon!.UserId = testGatewayAddCouponToUserRequestModel.UserId;
        testUserCoupon!.CouponId = testGatewayAddCouponToUserRequestModel.CouponId;
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(2);
        TestGatewayUserCoupon? currentlyAddedUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Code == testGatewayAddCouponToUserRequestModel.Code);
        currentlyAddedUserCoupon!.StartDate.Should().NotBeNull().And.Be(testGatewayAddCouponToUserRequestModel.StartDate);
        currentlyAddedUserCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testGatewayAddCouponToUserRequestModel.ExpirationDate);
    }

    [Test, Order(330)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(340)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(350)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(360)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(370)]
    public async Task UpdateUserCoupon_ShouldFailAndReturnNotFound_IfUserCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Id = "bogusUserCouponId";
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(380)]
    public async Task UpdateUserCoupon_ShouldSucceedAndUpdateUniversalUserCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateUserCouponRequestModel testUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testUpdateUserCouponRequestModel.Id = _chosenUniversalUserCouponId;
        testUpdateUserCouponRequestModel.Code = "MyNewUniversalUserCouponCode"; //this will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.TimesUsed = 1; //this is the main reason why this endpoint exists to begin with 
        testUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(1); //the dates will be overriden, because the coupon is universal
        testUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(2);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testUpdateUserCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{_chosenUniversalCouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(1);
        testCoupon!.UserCoupons[0].Code.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.Code);
        testCoupon!.UserCoupons[0].TimesUsed.Should().NotBeNull().And.Be(testUpdateUserCouponRequestModel.TimesUsed);
        testCoupon!.UserCoupons[0].StartDate.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.StartDate);
        testCoupon!.UserCoupons[0].ExpirationDate.Should().NotBeNull().And.NotBe(testUpdateUserCouponRequestModel.ExpirationDate);
    }

    [Test, Order(390)]
    public async Task UpdateUserCoupon_ShouldSucceedAndUpdateUserSpecificUserCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        //when it comes to user specific user coupons all the properties can be adjusted, so nothing will be overriden below
        TestGatewayUpdateUserCouponRequestModel testGatewayUpdateUserCouponRequestModel = new TestGatewayUpdateUserCouponRequestModel();
        testGatewayUpdateUserCouponRequestModel.Id = _chosenUserSpecificUserCouponId;
        testGatewayUpdateUserCouponRequestModel.Code = "MyNewUserSpecificUserCouponCode";
        testGatewayUpdateUserCouponRequestModel.TimesUsed = 1;
        testGatewayUpdateUserCouponRequestModel.StartDate = DateTime.Now.AddMonths(4);
        testGatewayUpdateUserCouponRequestModel.ExpirationDate = DateTime.Now.AddMonths(5);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCoupon/updateUserCoupon", testGatewayUpdateUserCouponRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayCoupon/{_chosenUserSpecificCouponId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCoupon!.UserCoupons.Should().NotBeNull().And.HaveCount(2);
        TestGatewayUserCoupon? currentlyUpdatedUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Code == testGatewayUpdateUserCouponRequestModel.Code);
        currentlyUpdatedUserCoupon!.Code.Should().NotBeNull().And.Be(testGatewayUpdateUserCouponRequestModel.Code);
        currentlyUpdatedUserCoupon!.TimesUsed.Should().NotBeNull().And.Be(testGatewayUpdateUserCouponRequestModel.TimesUsed);
        currentlyUpdatedUserCoupon!.StartDate.Should().NotBeNull().And.Be(testGatewayUpdateUserCouponRequestModel.StartDate);
        currentlyUpdatedUserCoupon!.ExpirationDate.Should().NotBeNull().And.Be(testGatewayUpdateUserCouponRequestModel.ExpirationDate);
    }

    [Test, Order(400)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{userCouponId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(410)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{userCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(420)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{userCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(430)]
    public async Task RemoveCouponFromUser_ShouldFailAndReturnNotFound_IfUserCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusUserCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{bogusUserCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(440)]
    public async Task RemoveCouponFromUser_ShouldSucceedAndRemoveCouponFromUser()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string? userCouponId = _chosenUniversalUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{userCouponId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayCoupon/removeCouponFromUser/userCouponId/{userCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(450)]
    public async Task DeleteCoupon_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/{couponId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(460)]
    public async Task DeleteCoupon_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/{couponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(470)]
    public async Task DeleteCoupon_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/{couponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(480)]
    public async Task DeleteCoupon_ShouldFailAndReturnNotFound_IfCouponNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusCouponId = "bogusCouponId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/{bogusCouponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(490)]
    public async Task DeleteCoupon_ShouldSucceedAndDeleteCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string couponId = _chosenUniversalCouponId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCoupon/{couponId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayCoupon/{couponId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(700)]
    public async Task GetCoupons_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayCoupon/amount/10/includeDeactivated/true");

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
