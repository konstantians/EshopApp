using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.PaymentOptionTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ShippingOptionTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayOrderControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _userId;
    private string? _chosenCouponId;
    private string? _chosenUserCouponId;
    private string? _chosenManagerCouponId;
    private string? _chosenPaymentOptionId;
    private string? _chosenShippingOptionId;
    private string? _chosenVariantId;
    private string? _otherChosenVariantId;
    private string? _chosenOrderId;

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

        //Get the user's userId
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        string? responseBody = await response.Content.ReadAsStringAsync();
        _userId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //Get the user's userId
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _managerAccessToken);
        response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken");
        responseBody = await response.Content.ReadAsStringAsync();
        string managerId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //Create coupon
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateCouponRequestModel testCreateCouponRequestModel = new TestGatewayCreateCouponRequestModel();
        testCreateCouponRequestModel.Code = "MyCouponCode"; //because this code already exists it should create a random different code
        testCreateCouponRequestModel.Description = "My universal coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 10;
        testCreateCouponRequestModel.UsageLimit = 1;
        testCreateCouponRequestModel.IsUserSpecific = false; //it is universal
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon", testCreateCouponRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenCouponId = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;

        //Add it to the user
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenCouponId;
        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenUserCouponId = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;

        //Add another coupon to the manager
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.UserId = managerId;
        testGatewayAddCouponToUserRequestModel.CouponId = _chosenCouponId;
        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenManagerCouponId = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;

        //Create a payment option
        TestGatewayCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "CashPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.ExtraCost = 3;
        response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testCreatePaymentOptionRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenPaymentOptionId = JsonSerializer.Deserialize<TestGatewayPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //Create a shipping option
        TestGatewayCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "ReceiveFromStoreShippingOption";
        testCreateShippingOptionRequestModel.ExtraCost = 0;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;
        response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testCreateShippingOptionRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenShippingOptionId = JsonSerializer.Deserialize<TestGatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //Create product and variant
        TestGatewayCreateProductRequestModel testCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProdyctName";
        testCreateProductRequestModel.Code = "MyProductCode";
        TestGatewayCreateVariantRequestModel testCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MyVariantSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testCreateProductRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        string? productId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _chosenVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0]!.Id;

        //Create another product and variant
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MySKU";
        testGatewayCreateVariantRequestModel.Price = 100m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 5;
        testGatewayCreateVariantRequestModel.IsThumbnailVariant = true;
        testGatewayCreateVariantRequestModel.ProductId = productId;
        response = await httpClient.PostAsJsonAsync("api/gatewayVariant", testGatewayCreateVariantRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherChosenVariantId = JsonSerializer.Deserialize<TestGatewayVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        //the access token still needs to passed in since the user uses the UserId field and the couponId field
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel); //email property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfNoOrderItemsInRequestModel()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        errorMessage.Should().NotBeNull().And.Be("TheOrderMustHaveAtLeastOneOrderItem");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(50)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfAlternateShippingAddressIsSelectedButNotAllAltPropertiesAreFilled()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress");
    }

    [Test, Order(60)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfInvalidPaymentOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = "bogusPaymentOptionId";
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidPaymentOption");
    }

    [Test, Order(70)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfInvalidShippingOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = "bogusShippingOptionId";
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidShippingOption");
    }

    [Test, Order(80)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnForbidden_IfInvalidCouponId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = "bogusUserCoupondId";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("UserAccountDoesNotContainThisCoupon");
    }

    [Test, Order(90)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnForbidden_IfUserDoesNotOwnCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenManagerCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("UserAccountDoesNotContainThisCoupon");
    }

    [Test, Order(100)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneInvalidVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = "bogusVariantId"
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(110)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneVariantWithInsufficientStock()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 1000000,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(120)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldSucceedAndCreateOrder()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        await Task.Delay(waitTimeInMillisecond);
        string? orderPlacedEmailContent = TestUtilitiesLibrary.EmailUtilities.ReadLastEmailFile(deleteEmailFile: true);

        HttpResponseMessage userResponse = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken?includeOrders=true&includeCoupons=true");
        string? userResponseBody = await userResponse.Content.ReadAsStringAsync();
        TestGatewayAppUser gatewayAppUser = JsonSerializer.Deserialize<TestGatewayAppUser>(userResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        orderPlacedEmailContent.Should().NotBeNull();
        gatewayAppUser.Orders[0].Should().NotBeNull();
        gatewayAppUser.Orders[0].Comment.Should().Be(testGatewayCreateOrderRequestModel.Comment);
        gatewayAppUser.Orders[0].OrderStatus.Should().Be("Confirmed");
        gatewayAppUser.Orders[0].OrderAddress.Should().NotBeNull();
        gatewayAppUser.Orders[0].OrderAddress!.Email.Should().Be(testGatewayCreateOrderRequestModel.Email);
        gatewayAppUser.Orders[0].OrderAddress!.FirstName.Should().Be(testGatewayCreateOrderRequestModel.FirstName);
        gatewayAppUser.Orders[0].OrderAddress!.LastName.Should().Be(testGatewayCreateOrderRequestModel.LastName);
        gatewayAppUser.Orders[0].OrderAddress!.Country.Should().Be(testGatewayCreateOrderRequestModel.Country);
        gatewayAppUser.Orders[0].OrderAddress!.City.Should().Be(testGatewayCreateOrderRequestModel.City);
        gatewayAppUser.Orders[0].OrderAddress!.PostalCode.Should().Be(testGatewayCreateOrderRequestModel.PostalCode);
        gatewayAppUser.Orders[0].OrderAddress!.Address.Should().Be(testGatewayCreateOrderRequestModel.Address);
        gatewayAppUser.Orders[0].OrderAddress!.PhoneNumber.Should().Be(testGatewayCreateOrderRequestModel.PhoneNumber);
        gatewayAppUser.Orders[0].OrderAddress!.IsShippingAddressDifferent.Should().Be(testGatewayCreateOrderRequestModel.IsShippingAddressDifferent);
        gatewayAppUser.Orders[0].OrderAddress!.AltFirstName.Should().Be(testGatewayCreateOrderRequestModel.AltFirstName);
        gatewayAppUser.Orders[0].OrderAddress!.AltLastName.Should().Be(testGatewayCreateOrderRequestModel.AltLastName);
        gatewayAppUser.Orders[0].OrderAddress!.AltCountry.Should().Be(testGatewayCreateOrderRequestModel.AltCountry);
        gatewayAppUser.Orders[0].OrderAddress!.AltCity.Should().Be(testGatewayCreateOrderRequestModel.AltCity);
        gatewayAppUser.Orders[0].OrderAddress!.AltPostalCode.Should().Be(testGatewayCreateOrderRequestModel.AltPostalCode);
        gatewayAppUser.Orders[0].OrderAddress!.AltAddress.Should().Be(testGatewayCreateOrderRequestModel.AltAddress);
        gatewayAppUser.Orders[0].OrderAddress!.AltPhoneNumber.Should().Be(testGatewayCreateOrderRequestModel.AltPhoneNumber);
        gatewayAppUser.Orders[0].UserId.Should().Be(testGatewayCreateOrderRequestModel.UserId);
        gatewayAppUser.Orders[0].PaymentDetails.Should().NotBeNull();
        gatewayAppUser.Orders[0].PaymentDetails!.PaymentCurrency.Should().Be("N/A");
        gatewayAppUser.Orders[0].PaymentDetails!.AmountPaidInEuro.Should().Be(0);
        gatewayAppUser.Orders[0].PaymentDetails!.NetAmountPaidInEuro.Should().Be(0);
        gatewayAppUser.Orders[0].PaymentDetails!.PaymentProcessorSessionId.Should().BeNull();
        gatewayAppUser.Orders[0].PaymentDetails!.PaymentOptionId.Should().Be(testGatewayCreateOrderRequestModel.PaymentOptionId);
        gatewayAppUser.Orders[0].PaymentDetails!.PaymentStatus.Should().Be("Pending");
        gatewayAppUser.Orders[0].OrderItems.Should().HaveCount(1);
        gatewayAppUser.Orders[0].OrderItems[0].Quantity.Should().Be(testGatewayCreateOrderRequestModel.OrderItemRequestModels[0].Quantity);
        gatewayAppUser.Orders[0].ShippingOptionId.Should().Be(testGatewayCreateOrderRequestModel.ShippingOptionId);
        gatewayAppUser.Orders[0].UserCouponId.Should().Be(testGatewayCreateOrderRequestModel.UserCouponId);

        testVariant!.UnitsInStock.Should().NotBeNull().And.Be(7); //the units in stock in this case change immediatelly since the order is confirmed
        testVariant.ExistsInOrder.Should().BeTrue();

        gatewayAppUser.UserCoupons[0].ExistsInOrder.Should().BeTrue();
        gatewayAppUser.UserCoupons[0].TimesUsed.Should().Be(1); //the timesUsed in this case change immediatelly since the order is confirmed

        _chosenOrderId = gatewayAppUser.Orders[0].Id;
    }

    [Test, Order(125)]
    public async Task CreateOrderWithoutCheckOutSession_ShouldFailAndReturnBadRequest_IfCouponHasUsageHasExceededMaxLimit()
    {
        //this is the same order essentially(still valid since there are enought units in stock), but this time the user has exceeded the max usage for the coupon

        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Comment = "UserComment";
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayCreateOrderRequestModel.AltFirstName = "Giannis";
        testGatewayCreateOrderRequestModel.AltLastName = "Kinnas";
        testGatewayCreateOrderRequestModel.AltCountry = "Greece";
        testGatewayCreateOrderRequestModel.AltCity = "Kos";
        testGatewayCreateOrderRequestModel.AltPostalCode = "85300";
        testGatewayCreateOrderRequestModel.AltAddress = "Apellou 6";
        testGatewayCreateOrderRequestModel.AltPhoneNumber = "6955555555";
        testGatewayCreateOrderRequestModel.UserId = _userId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels.Add(new TestGatewayOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayOrder", testGatewayCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("CouponUsageLimitExceeded");
    }

    [Test, Order(130)]
    public async Task GetOrders_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayOrder/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task GetOrders_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayOrder/amount/10"); //here 10 is just an arbitraty value for the amount

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(150)]
    public async Task GetOrders_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayOrder/amount/10"); //here 10 is just an arbitraty value for the amount

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(160)]
    public async Task GetOrders_ShouldSucceedAndReturnOrders()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayOrder/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayOrder>? testOrders = JsonSerializer.Deserialize<List<TestGatewayOrder>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testOrders.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(170)]
    public async Task GetOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayOrder/{orderId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(180)]
    public async Task GetOrder_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayOrder/{orderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(190)]
    public async Task GetOrder_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayOrder/{orderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(200)]
    public async Task GetOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusOrderId = "bogusOrderId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayOrder/{bogusOrderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(210)]
    public async Task GetOrder_ShouldSucceedAndReturnOrder()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayOrder/{orderId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayOrder? testOrder = JsonSerializer.Deserialize<TestGatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testOrder.Should().NotBeNull();
        testOrder!.Comment.Should().NotBeNull();
        testOrder.OrderStatus.Should().Be("Confirmed");
        testOrder!.OrderAddress.Should().NotBeNull();
        testOrder!.OrderAddress!.Email.Should().NotBeNull();
        testOrder!.OrderAddress.FirstName.Should().NotBeNull();
        testOrder!.OrderAddress.LastName.Should().NotBeNull();
        testOrder!.OrderAddress.Country.Should().NotBeNull();
        testOrder!.OrderAddress.City.Should().NotBeNull();
        testOrder!.OrderAddress.PostalCode.Should().NotBeNull();
        testOrder!.OrderAddress.Address.Should().NotBeNull();
        testOrder!.OrderAddress.PhoneNumber.Should().NotBeNull();
        testOrder!.OrderAddress.IsShippingAddressDifferent.Should().NotBeNull().And.BeTrue();
        testOrder!.OrderAddress.AltFirstName.Should().NotBeNull();
        testOrder!.OrderAddress.AltLastName.Should().NotBeNull();
        testOrder!.OrderAddress.AltCountry.Should().NotBeNull();
        testOrder!.OrderAddress.AltCity.Should().NotBeNull();
        testOrder!.OrderAddress.AltPostalCode.Should().NotBeNull();
        testOrder!.OrderAddress.AltAddress.Should().NotBeNull();
        testOrder!.OrderAddress.AltPhoneNumber.Should().NotBeNull();
        testOrder.UserId.Should().NotBeNull();
        testOrder.PaymentDetails.Should().NotBeNull();
        testOrder.PaymentDetails!.PaymentCurrency.Should().Be("N/A");
        testOrder.PaymentDetails!.AmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.NetAmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.PaymentOptionId.Should().NotBeNull();
        testOrder.PaymentDetails!.PaymentStatus.Should().Be("Pending");
        testOrder.OrderItems.Should().HaveCount(1);
        testOrder.OrderItems[0].Quantity.Should().NotBeNull();
        testOrder.OrderItems[0].UnitPriceAtOrder.Should().NotBeNull();
        testOrder.ShippingOptionId.Should().NotBeNull();
        testOrder.UserCouponId.Should().NotBeNull();
    }

    [Test, Order(220)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Processed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(230)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Processed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(240)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Processed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(250)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(260)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = "bogusOrderId";
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Processed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(270)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnBadRequest_IfInvalidNewOrderStatus()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Pending"; //the order status can not become from Confirmed to Pending

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InvalidNewOrderState");
    }

    [Test, Order(280)]
    public async Task UpdateOrderStatus_ShouldSucceedAndUpdateOrderStatus()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testGatewayUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testGatewayUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testGatewayUpdateOrderStatusRequestModel.NewOrderStatus = "Processed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testGatewayUpdateOrderStatusRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayOrder/{_chosenOrderId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayOrder? testOrder = JsonSerializer.Deserialize<TestGatewayOrder>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testOrder!.Should().NotBeNull();
        testOrder!.OrderStatus.Should().NotBeNull().And.Be(testGatewayUpdateOrderStatusRequestModel.NewOrderStatus);
    }

    [Test, Order(290)]
    public async Task UpdateOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testGatewayUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testGatewayUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testGatewayUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testGatewayUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testGatewayUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testGatewayUpdateOrderRequestModel.City = "UpdatedUserCity";
        testGatewayUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testGatewayUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testGatewayUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testGatewayUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testGatewayUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testGatewayUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testGatewayUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testGatewayUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testGatewayUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.Id = _chosenOrderId;
        testGatewayUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testGatewayUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testGatewayUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(300)]
    public async Task UpdateOrder_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateOrderRequestModel testGatewayUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testGatewayUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testGatewayUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testGatewayUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testGatewayUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testGatewayUpdateOrderRequestModel.City = "UpdatedUserCity";
        testGatewayUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testGatewayUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testGatewayUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testGatewayUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testGatewayUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testGatewayUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testGatewayUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testGatewayUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testGatewayUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.Id = _chosenOrderId;
        testGatewayUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testGatewayUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testGatewayUpdateOrderRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(310)]
    public async Task UpdateOrder_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateOrderRequestModel testGatewayUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testGatewayUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testGatewayUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testGatewayUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testGatewayUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testGatewayUpdateOrderRequestModel.City = "UpdatedUserCity";
        testGatewayUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testGatewayUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testGatewayUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testGatewayUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testGatewayUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testGatewayUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testGatewayUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testGatewayUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testGatewayUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.Id = _chosenOrderId;
        testGatewayUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testGatewayUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testGatewayUpdateOrderRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(320)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testGatewayUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testGatewayUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testGatewayUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testGatewayUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testGatewayUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testGatewayUpdateOrderRequestModel.City = "UpdatedUserCity";
        testGatewayUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testGatewayUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testGatewayUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testGatewayUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testGatewayUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testGatewayUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testGatewayUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testGatewayUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testGatewayUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testGatewayUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testGatewayUpdateOrderRequestModel.Id = _chosenOrderId;
        testGatewayUpdateOrderRequestModel.AmountPaidInEuro = -150;
        testGatewayUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testGatewayUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testGatewayUpdateOrderRequestModel); //the amounts can not be negative and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(330)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystemBasedOnOrderId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = "bogusOrderId";
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() { new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(340)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystemBasedOnPaymentProcessorSessionId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "bogusPaymentProcessorSessionId";
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("OrderNotFoundWithGivenPaymentProcessorSessionId");
    }

    [Test, Order(350)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystemBasedOnPaymentProcessorPaymentIntentId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.PaymentProcessorPaymentIntentId = "bogusPaymentProcessorPaymentIntentId";
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("OrderNotFoundWithGivenPaymentProcessorPaymentIntentId");
    }

    [Test, Order(360)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfTheOrderIdThePaymentProcessorSessionIdAndPaymentIntentIdAreAllNull()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("TheOrderIdThePaymentProcessorSessionIdAndPaymentIntentIdCanNotBeAllNull");
    }

    [Test, Order(370)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfInvalidUserCouponId()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = _chosenOrderId;
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = "bogusUserCouponId";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(380)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfThereIsAtLeastOneInvalidVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = _chosenOrderId;
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = "bogusVariantId" } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(390)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneVariantWithInsufficientStock()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = _chosenOrderId;
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 1000000, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(400)]
    public async Task UpdateOrder_ShouldSucceedAndUpdateOrder()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = _chosenOrderId; //find the order based on the id
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() {
            new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayOrder/{_chosenOrderId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayOrder? testOrder = JsonSerializer.Deserialize<TestGatewayOrder>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/gatewayvariant/id/{_chosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage otherVariantResponse = await httpClient.GetAsync($"api/gatewayvariant/id/{_otherChosenVariantId}/includeDeactivated/true");
        string? otherVariantResponseBody = await otherVariantResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? otherTestVariant = JsonSerializer.Deserialize<TestGatewayVariant>(otherVariantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        /*HttpResponseMessage couponResponse = await httpClient.GetAsync($"api/coupon/{_chosenCouponId}/includeDeactivated/true");
        string? couponResponseBody = await couponResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(couponResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        */

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testOrder.Should().NotBeNull();
        testOrder!.OrderAddress.Should().NotBeNull();
        testOrder!.OrderAddress!.Email.Should().Be(testUpdateOrderRequestModel.Email);
        testOrder!.OrderAddress.FirstName.Should().Be(testUpdateOrderRequestModel.FirstName);
        testOrder!.OrderAddress.LastName.Should().Be(testUpdateOrderRequestModel.LastName);
        testOrder!.OrderAddress.Country.Should().Be(testUpdateOrderRequestModel.Country);
        testOrder!.OrderAddress.City.Should().Be(testUpdateOrderRequestModel.City);
        testOrder!.OrderAddress.PostalCode.Should().Be(testUpdateOrderRequestModel.PostalCode);
        testOrder!.OrderAddress.Address.Should().Be(testUpdateOrderRequestModel.Address);
        testOrder!.OrderAddress.PhoneNumber.Should().Be(testUpdateOrderRequestModel.PhoneNumber);
        testOrder!.OrderAddress.IsShippingAddressDifferent.Should().Be(testUpdateOrderRequestModel.IsShippingAddressDifferent);
        testOrder!.OrderAddress.AltFirstName.Should().Be(testUpdateOrderRequestModel.AltFirstName);
        testOrder!.OrderAddress.AltLastName.Should().Be(testUpdateOrderRequestModel.AltLastName);
        testOrder!.OrderAddress.AltCountry.Should().Be(testUpdateOrderRequestModel.AltCountry);
        testOrder!.OrderAddress.AltCity.Should().Be(testUpdateOrderRequestModel.AltCity);
        testOrder!.OrderAddress.AltPostalCode.Should().Be(testUpdateOrderRequestModel.AltPostalCode);
        testOrder!.OrderAddress.AltAddress.Should().Be(testUpdateOrderRequestModel.AltAddress);
        testOrder!.OrderAddress.AltPhoneNumber.Should().Be(testUpdateOrderRequestModel.AltPhoneNumber);
        testOrder.PaymentDetails.Should().NotBeNull();
        testOrder.OrderItems.Should().HaveCount(1);
        testOrder.OrderItems[0].UnitPriceAtOrder.Should().Be(100);
        testOrder.OrderItems[0].Quantity.Should().Be(testUpdateOrderRequestModel.OrderItemRequestModels[0].Quantity);

        //The new variant should be changed
        otherTestVariant!.UnitsInStock.Should().NotBeNull().And.Be(0); //5 - 5 = 0 in this case
        otherTestVariant.ExistsInOrder.Should().BeTrue();

        //The previous testvariant should be reset
        testVariant!.UnitsInStock.Should().NotBeNull().And.Be(10);
        testVariant.ExistsInOrder.Should().BeFalse();

        /*TestUserCoupon previousOrderUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Id == _chosenUserCouponId)!;
        previousOrderUserCoupon.ExistsInOrder.Should().BeFalse();
        previousOrderUserCoupon.TimesUsed.Should().Be(0);*/
    }

    [Test, Order(410)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfTheOrderStatusHasBeenFinalized()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderRequestModel testUpdateOrderRequestModel = new TestGatewayUpdateOrderRequestModel();
        testUpdateOrderRequestModel.Email = "updatedUser@gmail.com";
        testUpdateOrderRequestModel.FirstName = "UpdatedUserFirstName";
        testUpdateOrderRequestModel.LastName = "UpdatedUserLastName";
        testUpdateOrderRequestModel.Country = "UpdatedUserCountry";
        testUpdateOrderRequestModel.City = "UpdatedUserCity";
        testUpdateOrderRequestModel.PostalCode = "UpdatedUserPostalCode";
        testUpdateOrderRequestModel.Address = "UpdatedUserAddress";
        testUpdateOrderRequestModel.PhoneNumber = "UpdatedUserPhoneNumber";
        testUpdateOrderRequestModel.IsShippingAddressDifferent = true;
        testUpdateOrderRequestModel.AltFirstName = "UpdatedAltUserFirstName";
        testUpdateOrderRequestModel.AltLastName = "UpdatedAltUserLastName";
        testUpdateOrderRequestModel.AltCountry = "UpdatedAltUserCountry";
        testUpdateOrderRequestModel.AltCity = "UpdatedAltUserCity";
        testUpdateOrderRequestModel.AltPostalCode = "UpdatedAltUserPostalCode";
        testUpdateOrderRequestModel.AltAddress = "UpdatedAltUserAddress";
        testUpdateOrderRequestModel.AltPhoneNumber = "UpdatedAltUserPhoneNumber";
        testUpdateOrderRequestModel.Id = _chosenOrderId; //find the order based on the id
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>() { new TestGatewayOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        //testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        TestGatewayUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Canceled";
        await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testUpdateOrderStatusRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered");
    }

    [Test, Order(420)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnBadRequest_IfTheOrderStatusHasBeenFinalized()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestGatewayUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Refunded";
        await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testUpdateOrderStatusRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayOrder/updateOrderStatus", testUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered");
    }

    [Test, Order(430)]
    public async Task DeleteOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayOrder/{orderId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(440)]
    public async Task DeleteOrder_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayOrder/{orderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(450)]
    public async Task DeleteOrder_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayOrder/{orderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(460)]
    public async Task DeleteOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusOrderId = "bogusOrderId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayOrder/{bogusOrderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(470)]
    public async Task DeleteOrder_ShouldSucceedAndDeleteOrder()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayOrder/{orderId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayOrder/{orderId}");

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/gatewayVariant/id/{_otherChosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestGatewayVariant? testVariant = JsonSerializer.Deserialize<TestGatewayVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage couponResponse = await httpClient.GetAsync($"api/gatewayCoupon/{_chosenCouponId}/includeDeactivated/true");
        string? couponResponseBody = await couponResponse.Content.ReadAsStringAsync();
        TestGatewayCoupon? testCoupon = JsonSerializer.Deserialize<TestGatewayCoupon>(couponResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        //Initial stock value. It did not change here since the status was already finalized in a status(Canceled) that resets the stock
        testVariant!.UnitsInStock.Should().NotBeNull().And.Be(5);
        testVariant!.ExistsInOrder.Should().BeFalse();

        TestGatewayUserCoupon orderUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Id == _chosenUserCouponId)!;
        orderUserCoupon.ExistsInOrder.Should().BeFalse();
        orderUserCoupon.TimesUsed.Should().Be(0);
    }

    //Rate Limit Test
    [Test, Order(700)]
    public async Task GetOrders_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayOrder/amount/10");

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
