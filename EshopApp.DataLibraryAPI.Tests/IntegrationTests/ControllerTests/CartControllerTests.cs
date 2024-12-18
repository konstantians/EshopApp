using EshopApp.DataLibrary.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CartModels;
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
internal class CartControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenCartId;
    private string? _chosenCartItemId;
    private string? _chosenProductId;
    private string? _chosenVariantId;
    private string? _otherVariantId;
    private string? _chosenUserId = "MyUserId";

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        /*************** Product & Variant ***************/
        TestCreateProductRequestModel testCreateProductRequestModel = new TestCreateProductRequestModel();
        testCreateProductRequestModel.Name = "MyProductName";
        testCreateProductRequestModel.Code = "MyProductCode";
        TestCreateVariantRequestModel testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "MyVariantSKU";
        testCreateVariantRequestModel.Price = 50m;
        testCreateVariantRequestModel.UnitsInStock = 10;
        testCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testCreateProductRequestModel.CreateVariantRequestModel = testCreateVariantRequestModel;
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/product", testCreateProductRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _chosenProductId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _chosenVariantId = JsonSerializer.Deserialize<TestProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;

        /*************** OtherVariant ***************/
        testCreateVariantRequestModel = new TestCreateVariantRequestModel();
        testCreateVariantRequestModel.SKU = "OtherVariantSKU";
        testCreateVariantRequestModel.Price = 30m;
        testCreateVariantRequestModel.UnitsInStock = 5;
        testCreateVariantRequestModel.ProductId = _chosenProductId;
        response = await httpClient.PostAsJsonAsync("api/variant", testCreateVariantRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _otherVariantId = JsonSerializer.Deserialize<TestVariant>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
    }

    [Test, Order(10)]
    public async Task CreateCart_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 2;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateCart_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 2;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateCart_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 2;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel); //the property userId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateCart_ShouldFailAndReturnNotFound_IfInvalidVariantIdWasGiven()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = "bogusVariantId";
        testCreateCartItemRequestModel.Quantity = 2;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(50)]
    public async Task CreateCart_ShouldFailAndReturnNotFound_IfThereIsInsufficientStockForGivenVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 10000000;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(60)]
    public async Task CreateCart_ShouldSucceedAndCreateCart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "WillBeOverriden";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 2;
        testCreateCartRequestModel.CreateCartItemRequestModels.Add(testCreateCartItemRequestModel);

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCart? testCart = JsonSerializer.Deserialize<TestCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCart.Should().NotBeNull();
        testCart!.Id.Should().NotBeNull();
        testCart!.CartItems.Should().NotBeNull().And.HaveCount(1);
        testCart!.CartItems[0].VariantId.Should().Be(testCreateCartItemRequestModel.VariantId);
        testCart!.CartItems[0].Quantity.Should().Be(testCreateCartItemRequestModel.Quantity);
        _chosenCartId = testCart.Id;
    }

    [Test, Order(70)]
    public async Task CreateCart_ShouldFailAndReturnBadRequest_IfUserAlreadyHasACart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId; //in the previous test a cart was created for that user

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("UserAlreadyHasACart");
    }

    [Test, Order(80)]
    public async Task GetCartById_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string cartId = _chosenCartId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/{cartId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetCartById_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCartId = "bogusCartId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/{bogusCartId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetCartById_ShouldSucceedAndReturnCart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string cartId = _chosenCartId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/{cartId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCart? testCart = JsonSerializer.Deserialize<TestCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCart.Should().NotBeNull();
        testCart!.Id.Should().NotBeNull().And.Be(cartId);
        testCart!.UserId.Should().NotBeNull().And.Be(_chosenUserId);
        testCart!.CartItems.Should().HaveCount(1);
        testCart!.CartItems[0].CartId.Should().Be(cartId);
    }

    [Test, Order(110)]
    public async Task GetCartOfUser_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string userId = _chosenUserId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/userId/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task GetCartOfUser_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/userId/{bogusUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(130)]
    public async Task GetCartOfUser_ShouldSucceedAndReturnCart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string userId = _chosenUserId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/cart/userId/{userId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCart? testCart = JsonSerializer.Deserialize<TestCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCart.Should().NotBeNull();
        testCart!.Id.Should().NotBeNull().And.Be(_chosenCartId);
        testCart!.UserId.Should().NotBeNull().And.Be(userId);
        testCart!.CartItems.Should().HaveCount(1);
        testCart!.CartItems[0].CartId.Should().Be(_chosenCartId);
    }

    [Test, Order(140)]
    public async Task CreateCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = _chosenCartId;
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task CreateCartItem_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel); //the property CartId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(160)]
    public async Task CreateCartItem_ShouldFailAndReturnNotFound_IfInvalidCartIdWasGiven()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = "bogusCartId";
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidCartIdWasGiven");
    }

    [Test, Order(170)]
    public async Task CreateCartItem_ShouldFailAndReturnNotFound_IfInvalidVariantIdWasGiven()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = _chosenCartId;
        testCreateCartItemRequestModel.VariantId = "bogusVariantId";
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(180)]
    public async Task CreateCartItem_ShouldFailAndReturnNotFound_IfThereIsInsufficientStockForGivenVariant()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = _chosenCartId;
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 1000000;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(190)]
    public async Task CreateCartItem_ShouldSucceedAndAdjustCartItems_IfTheGivenVariantIdExistsInsideAnAlreadyExistingCartItem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = _chosenCartId;
        testCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCartItem? testCartItem = JsonSerializer.Deserialize<TestCartItem>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCartItem.Should().NotBeNull();
        testCartItem!.CartId.Should().NotBeNull().And.Be(testCreateCartItemRequestModel.CartId);
        testCartItem!.VariantId.Should().NotBeNull().And.Be(testCreateCartItemRequestModel.VariantId);
        testCartItem!.Quantity.Should().NotBeNull().And.BeGreaterThan(testCreateCartItemRequestModel.Quantity);
    }

    [Test, Order(200)]
    public async Task CreateCartItem_ShouldSucceedAndCreateCartItem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateCartItemRequestModel testCreateCartItemRequestModel = new TestCreateCartItemRequestModel();
        testCreateCartItemRequestModel.CartId = _chosenCartId;
        testCreateCartItemRequestModel.VariantId = _otherVariantId;
        testCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/cart/cartItem", testCreateCartItemRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestCartItem? testCartItem = JsonSerializer.Deserialize<TestCartItem>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCartItem.Should().NotBeNull();
        testCartItem!.CartId.Should().NotBeNull().And.Be(testCreateCartItemRequestModel.CartId);
        testCartItem!.VariantId.Should().NotBeNull().And.Be(testCreateCartItemRequestModel.VariantId);
        testCartItem!.Quantity.Should().NotBeNull().And.Be(testCreateCartItemRequestModel.Quantity);
        _chosenCartItemId = testCartItem.Id;
    }

    [Test, Order(210)]
    public async Task UpdateCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateCartItemRequestModel testUpdateCreateCartItemRequestModel = new TestUpdateCartItemRequestModel();
        testUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/cart/cartItem", testUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task UpdateCartItem_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCartItemRequestModel testUpdateCreateCartItemRequestModel = new TestUpdateCartItemRequestModel();
        testUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/cart/cartItem", testUpdateCreateCartItemRequestModel); //the property CartItemId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(230)]
    public async Task UpdateCartItem_ShouldFailAndReturnNotFound_IfCartItemNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCartItemRequestModel testUpdateCreateCartItemRequestModel = new TestUpdateCartItemRequestModel();
        testUpdateCreateCartItemRequestModel.CartItemId = "bogusCartItemId";
        testUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/cart/cartItem", testUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(240)]
    public async Task UpdateCartItem_ShouldFailAndReturnNotFound_IfThereIsInsufficientStockForTheVariantOfTheCartItem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCartItemRequestModel testUpdateCreateCartItemRequestModel = new TestUpdateCartItemRequestModel();
        testUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testUpdateCreateCartItemRequestModel.Quantity = 1000000;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/cart/cartItem", testUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(250)]
    public async Task UpdateCartItem_ShouldSucceedAndUpdateCartItem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateCartItemRequestModel testUpdateCreateCartItemRequestModel = new TestUpdateCartItemRequestModel();
        testUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/cart/cartItem", testUpdateCreateCartItemRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/cart/{_chosenCartId}");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestCart? testCart = JsonSerializer.Deserialize<TestCart>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testCart.Should().NotBeNull();
        testCart!.CartItems.Should().HaveCount(2);
        TestCartItem? updatedCartItem = testCart!.CartItems.FirstOrDefault(cartItem => cartItem.Id == testUpdateCreateCartItemRequestModel.CartItemId);
        updatedCartItem.Should().NotBeNull();
        updatedCartItem!.Quantity.Should().Be(testUpdateCreateCartItemRequestModel.Quantity);
    }

    [Test, Order(260)]
    public async Task DeleteCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string cartItemId = _chosenCartItemId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/cartItem/{cartItemId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(270)]
    public async Task DeleteCartItem_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCartItemId = "bogusCartId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/cartItem/{bogusCartItemId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(280)]
    public async Task DeleteCartItem_ShouldSucceedAndDeleteCartItem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string cartItemId = _chosenCartItemId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/cartItem/{cartItemId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/cart/cartItem/{cartItemId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(290)]
    public async Task DeleteCartById_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string cartId = _chosenCartId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/{cartId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(300)]
    public async Task DeleteCartById_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusCartId = "bogusCartId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/{bogusCartId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(310)]
    public async Task DeleteCartById_ShouldSucceedAndDeleteCart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string cartId = _chosenCartId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/{cartId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/cart/{cartId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(320)]
    public async Task DeleteCartByUserId_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestCreateCartRequestModel testCreateCartRequestModel = new TestCreateCartRequestModel();
        testCreateCartRequestModel.UserId = _chosenUserId;
        await httpClient.PostAsJsonAsync("api/cart", testCreateCartRequestModel);

        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string userId = _chosenUserId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/userId/{userId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(330)]
    public async Task DeleteCartByUserId_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusUserId = "bogusUserId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/userId/{bogusUserId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(340)]
    public async Task DeleteCartByUserId_ShouldSucceedAndDeleteCart()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string userId = _chosenUserId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/cart/userId/{userId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/cart/userId/{userId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(400)]
    public async Task GetCartById_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string? cartId = _chosenCartId;

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync($"api/cart/{cartId}");

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
                "dbo.CartItems", "dbo.Carts"},
            "Data Database Successfully Cleared!"
        );
    }
}
