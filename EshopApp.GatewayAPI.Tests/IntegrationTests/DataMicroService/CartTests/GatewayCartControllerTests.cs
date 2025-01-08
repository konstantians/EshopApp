using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAdminTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CartTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CartTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayCartControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _otherUserAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenVariantId;
    private string? _userCartId;
    private string? _chosenCartItemId;

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
        HttpResponseMessage response = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken?includeCart=true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        _userCartId = JsonSerializer.Deserialize<TestGatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Cart!.Id;

        //sign up another user
        TestGatewayApiSignUpRequestModel signUpModel = new TestGatewayApiSignUpRequestModel();
        signUpModel.Email = "realag58@gmail.com";
        signUpModel.PhoneNumber = "6977777777";
        signUpModel.Password = "Kinas2020!";
        signUpModel.ClientUrl = "https://localhost:7070/controller/clientAction";
        await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signup", signUpModel);
        await Task.Delay(7000);
        string? confirmationEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(deleteEmailFile: true);
        try
        {
            using HttpClient tempHttpClient = new HttpClient();
            await tempHttpClient.GetAsync(confirmationEmailLink);
        }
        catch { }

        //get the other user's access token
        TestGatewayApiSignInRequestModel testGatewayApiSignInRequestModel = new TestGatewayApiSignInRequestModel();
        testGatewayApiSignInRequestModel.Email = "realag58@gmail.com";
        testGatewayApiSignInRequestModel.Password = "Kinas2020!";
        testGatewayApiSignInRequestModel.RememberMe = true;

        response = await httpClient.PostAsJsonAsync("api/gatewayAuthentication/signin", testGatewayApiSignInRequestModel);
        _otherUserAccessToken = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "accessToken");

        /*************** Product & Variant ***************/
        TestGatewayCreateProductRequestModel testGatewayCreateProductRequestModel = new TestGatewayCreateProductRequestModel();
        testGatewayCreateProductRequestModel.Name = "MyProductName";
        testGatewayCreateProductRequestModel.Code = "MyProductCode";
        TestGatewayCreateVariantRequestModel testGatewayCreateVariantRequestModel = new TestGatewayCreateVariantRequestModel();
        testGatewayCreateVariantRequestModel.SKU = "MyVariantSKU";
        testGatewayCreateVariantRequestModel.Price = 50m;
        testGatewayCreateVariantRequestModel.UnitsInStock = 10;
        testGatewayCreateVariantRequestModel.ProductId = "IdWillBeOverriden"; //This will be overriden in the dataaccess
        testGatewayCreateProductRequestModel.CreateVariantRequestModel = testGatewayCreateVariantRequestModel;
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        response = await httpClient.PostAsJsonAsync("api/gatewayProduct", testGatewayCreateProductRequestModel);
        responseBody = await response.Content.ReadAsStringAsync();
        _chosenVariantId = JsonSerializer.Deserialize<TestGatewayProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Variants[0].Id;
    }

    [Test, Order(10)]
    public async Task CreateCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(20)]
    public async Task CreateCartItem_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel); //the property CartId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(30)]
    public async Task CreateCartItem_ShouldFailAndReturnForbidden_IfInvalidCartIdWasGiven()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = "bogusCartId";
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartDoesNotBelongToGivenUser");
    }

    [Test, Order(40)]
    public async Task CreateCartItem_ShouldFailAndReturnForbidden_IfUserDoesNotOwnGivenCart()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _otherUserAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartDoesNotBelongToGivenUser");
    }

    [Test, Order(50)]
    public async Task CreateCartItem_ShouldFailAndReturnNotFound_IfInvalidVariantIdWasGiven()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = "bogusVariantId";
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("InvalidVariantIdWasGiven");
    }

    [Test, Order(60)]
    public async Task CreateCartItem_ShouldFailAndReturnNotFound_IfThereIsInsufficientStockForGivenVariant()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 1000000;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(70)]
    public async Task CreateCartItem_ShouldSucceedAndCreateCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 4;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCartItem? testCartItem = JsonSerializer.Deserialize<TestGatewayCartItem>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testCartItem.Should().NotBeNull();
        testCartItem!.CartId.Should().NotBeNull().And.Be(testGatewayCreateCartItemRequestModel.CartId);
        testCartItem!.VariantId.Should().NotBeNull().And.Be(testGatewayCreateCartItemRequestModel.VariantId);
        testCartItem!.Quantity.Should().NotBeNull().And.Be(testGatewayCreateCartItemRequestModel.Quantity);
        _chosenCartItemId = testCartItem.Id;
    }

    [Test, Order(80)]
    public async Task CreateCartItem_ShouldSucceedAndAdjustCartItems_IfTheGivenVariantIdExistsInsideAnAlreadyExistingCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateCartItemRequestModel testGatewayCreateCartItemRequestModel = new TestGatewayCreateCartItemRequestModel();
        testGatewayCreateCartItemRequestModel.CartId = _userCartId;
        testGatewayCreateCartItemRequestModel.VariantId = _chosenVariantId;
        testGatewayCreateCartItemRequestModel.Quantity = 2;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayCart/cartItem", testGatewayCreateCartItemRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayCartItem? testCartItem = JsonSerializer.Deserialize<TestGatewayCartItem>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testCartItem.Should().NotBeNull();
        testCartItem!.CartId.Should().NotBeNull().And.Be(testGatewayCreateCartItemRequestModel.CartId);
        testCartItem!.VariantId.Should().NotBeNull().And.Be(testGatewayCreateCartItemRequestModel.VariantId);
        testCartItem!.Quantity.Should().NotBeNull().And.BeGreaterThan(testGatewayCreateCartItemRequestModel.Quantity); //it should be 6 now(4+2), because before it was 2
    }

    [Test, Order(90)]
    public async Task UpdateCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(100)]
    public async Task UpdateCartItem_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel); //the property CartItemId is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(110)]
    public async Task UpdateCartItem_ShouldFailAndReturnForbidden_IfCartItemNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.CartItemId = "bogusCartItemId";
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartItemDoesNotBelongToGivenUser");
    }

    [Test, Order(120)]
    public async Task UpdateCartItem_ShouldFailAndReturnForbidden_IfUserDoesNotOwnGivenCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _otherUserAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartItemDoesNotBelongToGivenUser");
    }

    [Test, Order(130)]
    public async Task UpdateCartItem_ShouldFailAndReturnNotFound_IfThereIsInsufficientStockForTheVariantOfTheCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 100000;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("InsufficientStockForVariant");
    }

    [Test, Order(140)]
    public async Task UpdateCartItem_ShouldSucceedAndUpdateCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateCartItemRequestModel testGatewayUpdateCreateCartItemRequestModel = new TestGatewayUpdateCartItemRequestModel();
        testGatewayUpdateCreateCartItemRequestModel.CartItemId = _chosenCartItemId;
        testGatewayUpdateCreateCartItemRequestModel.Quantity = 1;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayCart/cartItem", testGatewayUpdateCreateCartItemRequestModel);
        HttpResponseMessage getUserByAccessTokenResponse = await httpClient.GetAsync("api/GatewayAuthentication/GetUserByAccessToken?includeCart=true");
        string? getUserByAccessTokenResponseBody = await getUserByAccessTokenResponse.Content.ReadAsStringAsync();
        TestGatewayCartItem chosenCartItem = JsonSerializer.Deserialize<TestGatewayAppUser>(getUserByAccessTokenResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Cart!.CartItems[0]!;

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        chosenCartItem.Should().NotBeNull();
        chosenCartItem.Id.Should().Be(testGatewayUpdateCreateCartItemRequestModel.CartItemId);
        chosenCartItem.Quantity.Should().Be(testGatewayUpdateCreateCartItemRequestModel.Quantity);
    }

    [Test, Order(150)]
    public async Task DeleteCartItem_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _userAccessToken);
        string cartItemId = _chosenCartItemId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{cartItemId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(160)]
    public async Task DeleteCartItem_ShouldFailAndReturnNotFound_IfCartNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string bogusCartItemId = "bogusCartId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{bogusCartItemId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartItemDoesNotBelongToGivenUser");
    }

    [Test, Order(170)]
    public async Task DeleteCartItem_ShouldFailAndReturnForbidden_IfUserDoesNotOwnGivenCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _otherUserAccessToken);
        string cartItemId = _chosenCartItemId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{cartItemId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        errorMessage.Should().NotBeNull().And.Be("GivenCartItemDoesNotBelongToGivenUser");
    }

    [Test, Order(180)]
    public async Task DeleteCartItem_ShouldSucceedAndDeleteCartItem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string cartItemId = _chosenCartItemId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{cartItemId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{cartItemId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task DeleteCartItem_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.DeleteAsync($"api/gatewayCart/cartItem/{_chosenCartItemId}");

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
