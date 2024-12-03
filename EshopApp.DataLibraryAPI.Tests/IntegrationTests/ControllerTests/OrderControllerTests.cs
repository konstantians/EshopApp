using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CouponModels;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.OrderModels;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.PaymentOptionModels;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.ShippingOptionModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.DiscountModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ImageModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ProductModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

//TODO add discount to variant
//TODO add the get, update, updatestatus and delete
[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class OrderControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenOrderId;
    private string? _chosenShippingOptionId;
    private string? _chosenPaymentOptionId;
    private string? _chosenProductId;
    private string? _chosenVariantId;
    private string? _otherChosenVariantId;
    private string? _chosenDiscountId;
    private string? _chosenImageId;
    private string? _chosenCouponId;
    private string? _chosenUserCouponId;
    private string? _otherChosenUserCouponId;
    private string? _otherUserCouponId;
    private string? _chosenUserId = "MyUserId";

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
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions" },
            "Data Database Successfully Cleared!"
        );

        /*************** Discount ***************/
        TestCreateDiscountRequestModel testCreateDiscountRequestModel = new TestCreateDiscountRequestModel();
        testCreateDiscountRequestModel.Name = "MyDiscount";
        testCreateDiscountRequestModel.Percentage = 20;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/discount", testCreateDiscountRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenDiscountId = JsonSerializer.Deserialize<TestDiscount>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Product & Variant ***************/
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "ProductName";
        testCreateProductRequestModel.Code = "ProductCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "ProductVariantSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateVariantRequestModel.DiscountId = _chosenDiscountId;
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        TestProduct chosenProduct = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        _chosenProductId = chosenProduct.Id;
        _chosenVariantId = chosenProduct.Variants.First().Id;

        /*************** Other Chosen Variant ***************/
        testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherChosenVariantSKU";
        testCreateVariantRequestModel.Price = 100m;
        testCreateVariantRequestModel.UnitsInStock = 20;
        testCreateVariantRequestModel.ProductId = _chosenProductId;

        response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherChosenVariantId = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Image ***************/
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "MyImage";
        testCreateImageRequestModel.ImagePath = "MyPath";
        response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenImageId = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** VariantImage ***************/
        TestUpdateVariantRequestModel testUpdateVariantRequestModel = new TestUpdateVariantRequestModel();
        testUpdateVariantRequestModel.Id = _chosenVariantId;
        testUpdateVariantRequestModel.ImagesIds = new() { _chosenImageId! };
        response = await httpClient.PutAsJsonAsync("api/variant", testUpdateVariantRequestModel);

        /*************** Universal Coupon ***************/
        TestCreateCouponRequestModel testCreateCouponRequestModel = new TestCreateCouponRequestModel();
        testCreateCouponRequestModel.Description = "My other coupon description";
        testCreateCouponRequestModel.DiscountPercentage = 10;
        testCreateCouponRequestModel.UsageLimit = 1;
        testCreateCouponRequestModel.IsUserSpecific = false;
        testCreateCouponRequestModel.IsDeactivated = false;
        testCreateCouponRequestModel.StartDate = DateTime.Now;
        testCreateCouponRequestModel.ExpirationDate = DateTime.Now.AddDays(2);

        response = await httpClient.PostAsJsonAsync("api/coupon", testCreateCouponRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenCouponId = JsonSerializer.Deserialize<TestCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** UserCoupon ***************/
        TestAddCouponToUserRequestModel testAddCouponToUserRequestModel = new TestAddCouponToUserRequestModel();
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenCouponId;

        response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenUserCouponId = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other Chosen UserCoupon ***************/
        testAddCouponToUserRequestModel.UserId = _chosenUserId;
        testAddCouponToUserRequestModel.CouponId = _chosenCouponId;

        response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherChosenUserCouponId = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Other UserCoupon(it belongs to another user) ***************/
        testAddCouponToUserRequestModel.UserId = "otherUserId";
        testAddCouponToUserRequestModel.CouponId = _chosenCouponId;

        response = await httpClient.PostAsJsonAsync("api/coupon/addCouponToUser", testAddCouponToUserRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherUserCouponId = JsonSerializer.Deserialize<TestUserCoupon>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Shipping Option ***************/
        TestCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testCreateShippingOptionRequestModel.ExtraCost = 5;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;

        response = await httpClient.PostAsJsonAsync("api/shippingOption", testCreateShippingOptionRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenShippingOptionId = JsonSerializer.Deserialize<TestShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;

        /*************** Payment Option ***************/
        TestCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "cash";
        testCreatePaymentOptionRequestModel.ExtraCost = 10;

        response = await httpClient.PostAsJsonAsync("api/paymentOption", testCreatePaymentOptionRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenPaymentOptionId = JsonSerializer.Deserialize<TestPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateOrder_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel); //email property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfNoOrderItemsInRequestModel()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;
        testCreateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>();

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        errorMessage.Should().NotBeNull().And.Be("TheOrderMustHaveAtLeastOneOrderItem");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(50)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfAlternateShippingAddressIsSelectedButNotAllAltPropertiesAreFilled()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel); //AltFirstName and AltLastName are missing, while IsShippingAddressDifferent property is true
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress");
    }

    [Test, Order(60)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfInvalidPaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = "bogusPaymentOptionId";
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidPaymentOption");
    }

    [Test, Order(70)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfInvalidShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = "bogusShippingOptionId";
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidShippingOption");
    }

    [Test, Order(80)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfInvalidCouponId()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = "bogusUserCouponId";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(90)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfUserDoesNotOwnCoupon()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _otherUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(100)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneInvalidVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 2,
            VariantId = "bogusVariantId"
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(110)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneVariantWithInsufficientStock()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 1000000,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(120)]
    public async Task CreateOrder_ShouldSucceedAndCreateOrder()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestOrder? testOrder = JsonSerializer.Deserialize<TestOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage couponResponse = await httpClient.GetAsync($"api/coupon/{_chosenCouponId}/includeDeactivated/true");
        string? couponResponseBody = await couponResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(couponResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testOrder.Should().NotBeNull();
        testOrder!.Comment.Should().Be(testCreateOrderRequestModel.Comment);
        testOrder.OrderStatus.Should().Be("Pending");
        testOrder!.OrderAddress.Should().NotBeNull();
        testOrder!.OrderAddress!.Email.Should().Be(testCreateOrderRequestModel.Email);
        testOrder!.OrderAddress.FirstName.Should().Be(testCreateOrderRequestModel.FirstName);
        testOrder!.OrderAddress.LastName.Should().Be(testCreateOrderRequestModel.LastName);
        testOrder!.OrderAddress.Country.Should().Be(testCreateOrderRequestModel.Country);
        testOrder!.OrderAddress.City.Should().Be(testCreateOrderRequestModel.City);
        testOrder!.OrderAddress.PostalCode.Should().Be(testCreateOrderRequestModel.PostalCode);
        testOrder!.OrderAddress.Address.Should().Be(testCreateOrderRequestModel.Address);
        testOrder!.OrderAddress.PhoneNumber.Should().Be(testCreateOrderRequestModel.PhoneNumber);
        testOrder!.OrderAddress.IsShippingAddressDifferent.Should().Be(testCreateOrderRequestModel.IsShippingAddressDifferent);
        testOrder!.OrderAddress.AltFirstName.Should().Be(testCreateOrderRequestModel.AltFirstName);
        testOrder!.OrderAddress.AltLastName.Should().Be(testCreateOrderRequestModel.AltLastName);
        testOrder!.OrderAddress.AltCountry.Should().Be(testCreateOrderRequestModel.AltCountry);
        testOrder!.OrderAddress.AltCity.Should().Be(testCreateOrderRequestModel.AltCity);
        testOrder!.OrderAddress.AltPostalCode.Should().Be(testCreateOrderRequestModel.AltPostalCode);
        testOrder!.OrderAddress.AltAddress.Should().Be(testCreateOrderRequestModel.AltAddress);
        testOrder!.OrderAddress.AltPhoneNumber.Should().Be(testCreateOrderRequestModel.AltPhoneNumber);
        testOrder.UserId.Should().Be(testCreateOrderRequestModel.UserId);
        testOrder.PaymentDetails.Should().NotBeNull();
        testOrder.PaymentDetails!.PaymentCurrency.Should().Be("N/A");
        testOrder.PaymentDetails!.AmountPaidInCustomerCurrency.Should().Be(0);
        testOrder.PaymentDetails!.AmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.NetAmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.PaymentProcessorSessionId.Should().Be(testCreateOrderRequestModel.PaymentProcessorSessionId);
        testOrder.PaymentDetails!.PaymentOptionId.Should().Be(testCreateOrderRequestModel.PaymentOptionId);
        testOrder.PaymentDetails!.PaymentStatus.Should().Be("Pending");
        testOrder.OrderItems.Should().HaveCount(1);
        testOrder.OrderItems[0].Quantity.Should().Be(testCreateOrderRequestModel.OrderItemRequestModels[0].Quantity);
        testOrder.OrderItems[0].UnitPriceAtOrder.Should().Be(testVariant!.Price - (testVariant.Price * testVariant.Discount!.Percentage / 100));
        testOrder.ShippingOptionId.Should().Be(testCreateOrderRequestModel.ShippingOptionId);
        testOrder.UserCouponId.Should().Be(testCreateOrderRequestModel.UserCouponId);

        testVariant!.UnitsInStock.Should().NotBeNull().And.BeLessThan(10);
        testVariant.ExistsInOrder.Should().BeTrue();
        testVariant.VariantImages[0].Image!.ExistsInOrder.Should().BeTrue();

        TestUserCoupon chosenUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Id == _chosenUserCouponId)!;
        chosenUserCoupon.ExistsInOrder.Should().BeTrue();
        chosenUserCoupon.TimesUsed.Should().Be(1);

        _chosenOrderId = testOrder.Id;
    }

    [Test, Order(130)]
    public async Task CreateOrder_ShouldFailAndReturnBadRequest_IfCouponHasUsageHasExceededMaxLimit()
    {
        //this is the same order essentially(still valid since there are enought units in stock), but this time the user has exceeded the max usage for the coupon

        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateOrderRequestModel testCreateOrderRequestModel = new TestCreateOrderRequestModel();
        testCreateOrderRequestModel.Comment = "UserComment";
        testCreateOrderRequestModel.Email = "user@gmail.com";
        testCreateOrderRequestModel.FirstName = "UserFirstName";
        testCreateOrderRequestModel.LastName = "UserLastName";
        testCreateOrderRequestModel.Country = "UserCountry";
        testCreateOrderRequestModel.City = "UserCity";
        testCreateOrderRequestModel.PostalCode = "UserPostalCode";
        testCreateOrderRequestModel.Address = "UserAddress";
        testCreateOrderRequestModel.PhoneNumber = "UserPhoneNumber";
        testCreateOrderRequestModel.IsShippingAddressDifferent = true;
        testCreateOrderRequestModel.AltFirstName = "AltUserFirstName";
        testCreateOrderRequestModel.AltLastName = "AltUserLastName";
        testCreateOrderRequestModel.AltCountry = "AltUserCountry";
        testCreateOrderRequestModel.AltCity = "AltUserCity";
        testCreateOrderRequestModel.AltPostalCode = "AltUserPostalCode";
        testCreateOrderRequestModel.AltAddress = "AltUserAddress";
        testCreateOrderRequestModel.AltPhoneNumber = "AltUserPhoneNumber";
        testCreateOrderRequestModel.UserId = _chosenUserId;
        testCreateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId";
        testCreateOrderRequestModel.OrderItemRequestModels.Add(new TestOrderItemRequestModel()
        {
            Quantity = 3,
            VariantId = _chosenVariantId
        });
        testCreateOrderRequestModel.PaymentOptionId = _chosenPaymentOptionId;
        testCreateOrderRequestModel.ShippingOptionId = _chosenShippingOptionId;
        testCreateOrderRequestModel.UserCouponId = _chosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/order", testCreateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("CouponUsageLimitExceeded");
    }

    [Test, Order(140)]
    public async Task GetOrders_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/order/amount/10"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task GetOrders_ShouldSucceedAndReturnOrders()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/order/amount/10");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestOrder>? testOrders = JsonSerializer.Deserialize<List<TestOrder>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testOrders.Should().NotBeNull().And.HaveCount(1);
    }

    [Test, Order(160)]
    public async Task GetOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/order/{orderId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(170)]
    public async Task GetOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusOrderId = "bogusOrderId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/order/{bogusOrderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(180)]
    public async Task GetOrder_ShouldSucceedAndReturnOrder()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/order/{orderId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestOrder? testOrder = JsonSerializer.Deserialize<TestOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testOrder.Should().NotBeNull();
        testOrder!.Comment.Should().NotBeNull();
        testOrder.OrderStatus.Should().Be("Pending");
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
        testOrder.PaymentDetails!.AmountPaidInCustomerCurrency.Should().Be(0);
        testOrder.PaymentDetails!.AmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.NetAmountPaidInEuro.Should().Be(0);
        testOrder.PaymentDetails!.PaymentProcessorSessionId.Should().NotBeNull();
        testOrder.PaymentDetails!.PaymentOptionId.Should().NotBeNull();
        testOrder.PaymentDetails!.PaymentStatus.Should().Be("Pending");
        testOrder.OrderItems.Should().HaveCount(1);
        testOrder.OrderItems[0].Quantity.Should().NotBeNull();
        testOrder.OrderItems[0].UnitPriceAtOrder.Should().NotBeNull();
        testOrder.ShippingOptionId.Should().NotBeNull();
        testOrder.UserCouponId.Should().NotBeNull();
    }

    [Test, Order(190)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Confirmed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(200)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel); //NewOrderStatus property is missing

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(210)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = "bogusOrderId";
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Confirmed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(220)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnBadRequest_IfInvalidNewOrderStatus()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Shipped"; //the order status can not become from Pending to Shipped

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InvalidNewOrderState");
    }

    [Test, Order(230)]
    public async Task UpdateOrderStatus_ShouldSucceedAndUpdateOrderStatus()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Confirmed";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/order/{_chosenOrderId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestOrder? testOrder = JsonSerializer.Deserialize<TestOrder>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testOrder!.Should().NotBeNull();
        testOrder!.OrderStatus.Should().NotBeNull().And.Be(testUpdateOrderStatusRequestModel.NewOrderStatus);
    }

    [Test, Order(240)]
    public async Task UpdateOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(250)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = -150;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel); //the amounts can not be negative and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(260)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystemBasedOnOrderId()
    {
        //Arrange
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m; //this was the amount?
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(270)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystemBasedOnPaymentProcessorSessionId()
    {
        //Arrange
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m; //this was the amount?
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("OrderNotFoundWithGivenPaymentProcessorSessionId");
    }

    [Test, Order(280)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfBothOrderIdAndPaymentProcessorSessionIdAreNull()
    {
        //Arrange
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("TheOrderIdAndThePaymentProcessorSessionIdCanNotBeBothNull");
    }

    [Test, Order(290)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfInvalidUserCouponId()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = "bogusUserCouponId";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCouponIdWasGiven");
    }

    [Test, Order(300)]
    public async Task UpdateOrder_ShouldFailAndReturnNotFound_IfThereIsAtLeastOneInvalidVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = "bogusVariantId" } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(310)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfThereIsAtLeastOneVariantWithInsufficientStock()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 1000000, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(320)]
    public async Task UpdateOrder_ShouldSucceedAndUpdateOrder()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/order/{_chosenOrderId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestOrder? testOrder = JsonSerializer.Deserialize<TestOrder>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/variant/id/{_chosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage couponResponse = await httpClient.GetAsync($"api/coupon/{_chosenCouponId}/includeDeactivated/true");
        string? couponResponseBody = await couponResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(couponResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
        testOrder.PaymentDetails!.PaymentCurrency.Should().Be(testUpdateOrderRequestModel.PaymentCurrency);
        testOrder.PaymentDetails!.AmountPaidInCustomerCurrency.Should().Be(testUpdateOrderRequestModel.AmountPaidInCustomerCurrency);
        testOrder.PaymentDetails!.AmountPaidInEuro.Should().Be(testUpdateOrderRequestModel.AmountPaidInEuro);
        testOrder.PaymentDetails!.NetAmountPaidInEuro.Should().Be(testUpdateOrderRequestModel.NetAmountPaidInEuro);
        testOrder.PaymentDetails!.PaymentProcessorSessionId.Should().Be(testUpdateOrderRequestModel.PaymentProcessorSessionId);
        testOrder.PaymentDetails!.PaymentStatus.Should().Be(testUpdateOrderRequestModel.PaymentStatus);
        testOrder.OrderItems.Should().HaveCount(1);
        testOrder.OrderItems[0].Discount.Should().BeNull();
        testOrder.OrderItems[0].DiscountId.Should().BeNull();
        testOrder.OrderItems[0].UnitPriceAtOrder.Should().Be(100);
        testOrder.OrderItems[0].Quantity.Should().Be(testUpdateOrderRequestModel.OrderItemRequestModels[0].Quantity);

        testVariant!.UnitsInStock.Should().NotBeNull().And.Be(10);
        testVariant.ExistsInOrder.Should().BeFalse();
        testVariant.VariantImages[0].Image!.ExistsInOrder.Should().BeFalse();

        TestUserCoupon previousOrderUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Id == _chosenUserCouponId)!;
        previousOrderUserCoupon.ExistsInOrder.Should().BeFalse();
        previousOrderUserCoupon.TimesUsed.Should().Be(0);
    }

    [Test, Order(330)]
    public async Task UpdateOrder_ShouldFailAndReturnBadRequest_IfTheOrderStatusHasBeenFinalized()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateOrderRequestModel testUpdateOrderRequestModel = new TestUpdateOrderRequestModel();
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
        testUpdateOrderRequestModel.PaymentProcessorSessionId = "paymentProcessorSessionId"; //find the order based on the paymentProcessorSessionId
        testUpdateOrderRequestModel.PaymentStatus = "Paid";
        testUpdateOrderRequestModel.PaymentCurrency = "eur";
        testUpdateOrderRequestModel.AmountPaidInCustomerCurrency = 193.5m;
        testUpdateOrderRequestModel.AmountPaidInEuro = 193.5m;
        testUpdateOrderRequestModel.NetAmountPaidInEuro = 170m; //theoretically this is calculated after stripe fee
        testUpdateOrderRequestModel.OrderItemRequestModels = new List<TestOrderItemRequestModel>() { new TestOrderItemRequestModel() { Quantity = 5, VariantId = _otherChosenVariantId } };
        testUpdateOrderRequestModel.UserCouponId = _otherChosenUserCouponId;

        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Canceled";
        await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order", testUpdateOrderRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered");
    }

    [Test, Order(340)]
    public async Task UpdateOrderStatus_ShouldFailAndReturnBadRequest_IfTheOrderStatusHasBeenFinalized()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        TestUpdateOrderStatusRequestModel testUpdateOrderStatusRequestModel = new TestUpdateOrderStatusRequestModel();
        testUpdateOrderStatusRequestModel.OrderId = _chosenOrderId;
        testUpdateOrderStatusRequestModel.NewOrderStatus = "Refunded";
        await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/order/updateOrderStatus", testUpdateOrderStatusRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered");
    }

    [Test, Order(350)]
    public async Task DeleteOrder_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/order/{orderId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(360)]
    public async Task DeleteOrder_ShouldFailAndReturnNotFound_IfOrderNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusOrderId = "bogusOrderId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/order/{bogusOrderId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //TODO Other than that rerun all the tests and maybe check the other entity controller tests now that the order tests have been added
    [Test, Order(370)]
    public async Task DeleteOrder_ShouldSucceedAndDeleteOrder()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string orderId = _chosenOrderId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/order/{orderId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/order/{orderId}");

        HttpResponseMessage variantResponse = await httpClient.GetAsync($"api/variant/id/{_otherChosenVariantId}/includeDeactivated/true");
        string? variantResponseBody = await variantResponse.Content.ReadAsStringAsync();
        TestVariant? testVariant = JsonSerializer.Deserialize<TestVariant>(variantResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        HttpResponseMessage couponResponse = await httpClient.GetAsync($"api/coupon/{_chosenCouponId}/includeDeactivated/true");
        string? couponResponseBody = await couponResponse.Content.ReadAsStringAsync();
        TestCoupon? testCoupon = JsonSerializer.Deserialize<TestCoupon>(couponResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        testVariant!.UnitsInStock.Should().NotBeNull().And.Be(20);
        testVariant!.ExistsInOrder.Should().BeFalse();

        TestUserCoupon orderUserCoupon = testCoupon!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Id == _otherChosenUserCouponId)!;
        orderUserCoupon.ExistsInOrder.Should().BeFalse();
        orderUserCoupon.TimesUsed.Should().Be(0);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetOrders_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/order/amount/10");

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
