using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.PaymentOptionTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ShippingOptionTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.CheckOutSessionTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.TransactionMicroService.CheckOutSessionTests;
internal class GatewayCheckOutSessionControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _userId;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenUserCouponId;
    private string? _chosenManagerCouponId;
    private string? _chosenCouponCode;
    private string? _chosenPaymentOptionId;
    private string? _chosenShippingOptionId;
    private string? _chosenVariantId;
    private TestGatewayOrder? _chosenTestOrder;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        //set up microservices and do cleanup
        await HelperMethods.CommonProcedures.CommonProcessManagementDatabaseAndEmailCleanupAsync(false, true, true, true, true, false);

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
        string chosenCouponId = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;
        _chosenCouponCode = JsonSerializer.Deserialize<TestGatewayCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Code!;

        //Add it to the user
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayAddCouponToUserRequestModel testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.UserId = _userId;
        testGatewayAddCouponToUserRequestModel.CouponId = chosenCouponId;
        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenUserCouponId = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;

        //Add another coupon to the manager
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        testGatewayAddCouponToUserRequestModel = new TestGatewayAddCouponToUserRequestModel();
        testGatewayAddCouponToUserRequestModel.UserId = managerId;
        testGatewayAddCouponToUserRequestModel.CouponId = chosenCouponId;
        response = await httpClient.PostAsJsonAsync("api/gatewayCoupon/addCouponToUser", testGatewayAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenManagerCouponId = JsonSerializer.Deserialize<TestGatewayUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id!;

        //Create a payment option
        TestGatewayCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "CardPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "card";
        testCreatePaymentOptionRequestModel.ExtraCost = 4;
        response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testCreatePaymentOptionRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenPaymentOptionId = JsonSerializer.Deserialize<TestGatewayPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        //Create a shipping option
        TestGatewayCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testCreateShippingOptionRequestModel.ExtraCost = 5;
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
        _chosenVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0]!.Id;
    }

    [Test, Order(10)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel); //Without the create order model, the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfSuccessUrlOrCancelUrlAreNotTrusted()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://NotTrustedDomain/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://NotTrustedDomain/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OriginForSuccessUrlIsNotTrusted");
    }

    [Test, Order(50)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnUnauthorized_IfUserCouponCodeIsProvidedButNoAccessTokenIsProvided()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(60)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnForbidden_IfUserCouponCodeIsProvidedAndUserDoesNotOwnCoupon()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenManagerCouponId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("UserAccountDoesNotContainThisCoupon");
    }

    [Test, Order(70)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfNoOrderItemsWereProvided()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("TheOrderMustHaveAtLeastOneOrderItem");
    }

    [Test, Order(80)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfShippingAddressIsDifferentButNotAllAltFieldsAreFilled()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress");
    }

    [Test, Order(90)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.ShippingOptionId = "bogusShippingOptionId";
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidShippingOption");
    }

    [Test, Order(100)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.PaymentOptionId = "bogusPaymentOptionId";
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testGatewayCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidPaymentOption");
    }

    [Test, Order(110)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnNotFound_IfOneOfTheVariantsNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = "bogusVariantId",
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(110)]
    public async Task CreateCheckOutSession_ShouldFailAndReturnBadRequest_IfOneOfTheVariantsHasInsufficientStock()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 1000000,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    //this specific test creates an actualy stripe checkout(on test mode in development), so do not spam that
    [Test, Order(120)]
    public async Task CreateCheckOutSession_ShouldSucceedAndCreateCheckOutSession()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
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
        testGatewayCreateOrderRequestModel.OrderItemRequestModels = new List<TestGatewayOrderItemRequestModel>()
        {
            new TestGatewayOrderItemRequestModel()
            {
                VariantId = _chosenVariantId,
                Quantity = 2,
            }
        };
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);
        string? checkOutSessionUrl = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "checkOutSessionUrl");

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        HttpResponseMessage getOrderResponse = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken?includeOrders=true");
        string? getOrderResponseBody = await getOrderResponse.Content.ReadAsStringAsync();
        _chosenTestOrder = JsonSerializer.Deserialize<TestGatewayAppUser>(getOrderResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Orders[0];

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        checkOutSessionUrl.Should().NotBeNull();
    }

    [Test, Order(130)]
    public async Task HandleCreateCheckOutSession_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        var testGatewayHandleCreateCheckOutSessionRequestModel = new TestGatewayHandleCreateCheckOutSessionRequestModel();
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorSessionId = _chosenTestOrder!.PaymentDetails!.PaymentProcessorSessionId;
        //paymentIntentId would be filled by the transaction library with Stripe data
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorPaymentIntentId = "myPaymentIntentId";
        testGatewayHandleCreateCheckOutSessionRequestModel.NewOrderStatus = "Confirmed";
        testGatewayHandleCreateCheckOutSessionRequestModel.NewPaymentStatus = "paid";
        testGatewayHandleCreateCheckOutSessionRequestModel.ShouldSendEmail = false;
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentCurrency = "eur";
        testGatewayHandleCreateCheckOutSessionRequestModel.AmountPaidInEuro = 90;
        testGatewayHandleCreateCheckOutSessionRequestModel.NetAmountPaidInEuro = 80; //this would be filled by the transaction library with Stripe data

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession/HandleCreateCheckOutSession", testGatewayHandleCreateCheckOutSessionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task HandleCreateCheckOutSession_ShouldSucceedAndUpdateTheOrderCorrectlyIfSessionWasSuccessful()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);

        //This is the simulated data we are going to get from the transaction api request after a successful transaction has occured
        var testGatewayHandleCreateCheckOutSessionRequestModel = new TestGatewayHandleCreateCheckOutSessionRequestModel();
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorSessionId = _chosenTestOrder!.PaymentDetails!.PaymentProcessorSessionId;
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorPaymentIntentId = "myPaymentIntentId"; //this would be filled by the transaction library with Stripe data
        testGatewayHandleCreateCheckOutSessionRequestModel.NewOrderStatus = "Confirmed";
        testGatewayHandleCreateCheckOutSessionRequestModel.NewPaymentStatus = "paid";
        testGatewayHandleCreateCheckOutSessionRequestModel.ShouldSendEmail = false;
        testGatewayHandleCreateCheckOutSessionRequestModel.PaymentCurrency = "eur";
        testGatewayHandleCreateCheckOutSessionRequestModel.AmountPaidInEuro = 90;
        testGatewayHandleCreateCheckOutSessionRequestModel.NetAmountPaidInEuro = 80; //this would be filled by the transaction library with Stripe data

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession/HandleCreateCheckOutSession", testGatewayHandleCreateCheckOutSessionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task CreateCheckOutSession_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateCheckOutSessionRequestModel testCreateCheckOutSessionRequestModel = new TestGatewayCreateCheckOutSessionRequestModel();
        testCreateCheckOutSessionRequestModel.SuccessUrl = "https://localhost:7070/clientSuccessEndpoint";
        testCreateCheckOutSessionRequestModel.CancelUrl = "https://localhost:7070/clientFailureEndpoint";

        TestGatewayCreateOrderRequestModel testGatewayCreateOrderRequestModel = new TestGatewayCreateOrderRequestModel();
        testGatewayCreateOrderRequestModel.Email = "kinnaskonstantinos0@gmail.com";
        testGatewayCreateOrderRequestModel.FirstName = "Konstantinos";
        testGatewayCreateOrderRequestModel.LastName = "Kinnas";
        testGatewayCreateOrderRequestModel.Country = "Greece";
        testGatewayCreateOrderRequestModel.City = "Athens";
        testGatewayCreateOrderRequestModel.PostalCode = "11631";
        testGatewayCreateOrderRequestModel.Address = "Artemonos 57A";
        testGatewayCreateOrderRequestModel.PhoneNumber = "6943655624";
        testGatewayCreateOrderRequestModel.IsShippingAddressDifferent = false;
        testGatewayCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testGatewayCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateCheckOutSessionRequestModel.GatewayCreateOrderRequestModel = testGatewayCreateOrderRequestModel;

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.PostAsJsonAsync("api/gatewayCheckOutSession", testCreateCheckOutSessionRequestModel);

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
