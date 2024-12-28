using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ShippingOptionTests.Models.RequestModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ShippingOptionTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayShippingOptionControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenShippingOptionId;
    private string? _otherShippingOptionId;
    private string? _otherShippingOptionName;

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

        /*************** Other Shipping Option ***************/
        TestGatewayCreateShippingOptionRequestModel testCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testCreateShippingOptionRequestModel.Name = "OtherShippingOption";
        testCreateShippingOptionRequestModel.ExtraCost = 5;
        testCreateShippingOptionRequestModel.ContainsDelivery = true;

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testCreateShippingOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        _otherShippingOptionId = JsonSerializer.Deserialize<TestGatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Id;
        _otherShippingOptionName = JsonSerializer.Deserialize<TestGatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateShippingOption_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateShippingOption_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel); //name property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateShippingOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateShippingOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateShippingOption_ShouldFailAndReturnBadRequest_IfDuplicateShippingOptionName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = _otherShippingOptionName;
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(70)]
    public async Task CreateShippingOption_ShouldSucceedAndCreateShippingOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateShippingOptionRequestModel testGatewayCreateShippingOptionRequestModel = new TestGatewayCreateShippingOptionRequestModel();
        testGatewayCreateShippingOptionRequestModel.Name = "MyShippingOption";
        testGatewayCreateShippingOptionRequestModel.Description = "My shipping option description";
        testGatewayCreateShippingOptionRequestModel.ExtraCost = 3;
        testGatewayCreateShippingOptionRequestModel.ContainsDelivery = true;
        testGatewayCreateShippingOptionRequestModel.IsDeactivated = false;
        testGatewayCreateShippingOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayShippingOption", testGatewayCreateShippingOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayShippingOption? testShippingOption = JsonSerializer.Deserialize<TestGatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testShippingOption.Should().NotBeNull();
        testShippingOption!.Id.Should().NotBeNull();
        testShippingOption!.Name.Should().NotBeNull().And.Be(testGatewayCreateShippingOptionRequestModel.Name);
        testShippingOption!.Description.Should().NotBeNull().And.Be(testGatewayCreateShippingOptionRequestModel.Description);
        testShippingOption!.ExtraCost.Should().NotBeNull().And.Be(testGatewayCreateShippingOptionRequestModel.ExtraCost);
        testShippingOption.ContainsDelivery.Should().BeTrue();
        testShippingOption.IsDeactivated.Should().BeFalse();
        testShippingOption.ExistsInOrder.Should().BeFalse();
        _chosenShippingOptionId = testShippingOption.Id;
    }

    [Test, Order(80)]
    public async Task GetShippingOptions_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayShippingOption/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetShippingOptions_ShouldSucceedAndReturnShippingOptions()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayShippingOption/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayShippingOption>? testShippingOptions = JsonSerializer.Deserialize<List<TestGatewayShippingOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testShippingOptions.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(100)]
    public async Task GetShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayShippingOption/{shippingOptionId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusShippingOptionId = "bogusShippingOptionId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayShippingOption/{bogusShippingOptionId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task GetShippingOption_ShouldSucceedAndReturnShippingOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayShippingOption/{shippingOptionId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayShippingOption? testShippingOption = JsonSerializer.Deserialize<TestGatewayShippingOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(130)]
    public async Task UpdateShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task UpdateShippingOption_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(150)]
    public async Task UpdateShippingOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(160)]
    public async Task UpdateShippingOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task UpdateShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = "bogusShippingOptionId";
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateShippingOption_ShouldFailAndReturnBadRequest_IfDuplicateShippingOptionName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testGatewayUpdateShippingOptionRequestModel.Name = _otherShippingOptionName;
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateShippingOption_ShouldSucceedAndUpdateShippingOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateShippingOptionRequestModel testGatewayUpdateShippingOptionRequestModel = new TestGatewayUpdateShippingOptionRequestModel();
        testGatewayUpdateShippingOptionRequestModel.Id = _chosenShippingOptionId;
        testGatewayUpdateShippingOptionRequestModel.Name = "MyShippingOptionUpdated";
        testGatewayUpdateShippingOptionRequestModel.Description = "My shipping option description updated";
        testGatewayUpdateShippingOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayShippingOption", testGatewayUpdateShippingOptionRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayShippingOption/{_chosenShippingOptionId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayShippingOption? testShippingOption = JsonSerializer.Deserialize<TestGatewayShippingOption>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testShippingOption!.Name.Should().NotBeNull().And.Be(testGatewayUpdateShippingOptionRequestModel.Name);
        testShippingOption!.Description.Should().NotBeNull().And.Be(testGatewayUpdateShippingOptionRequestModel.Description);
        testShippingOption!.ExtraCost.Should().NotBeNull().And.Be(testGatewayUpdateShippingOptionRequestModel.ExtraCost);
        testShippingOption.ContainsDelivery.Should().BeTrue();
        testShippingOption.IsDeactivated.Should().BeFalse();
        testShippingOption.ExistsInOrder.Should().BeFalse();
        //testShippingOption.Orders.Should().NotBeNull().And.HaveCount(0);
    }

    [Test, Order(200)]
    public async Task DeleteShippingOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayShippingOption/{shippingOptionId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(210)]
    public async Task DeleteShippingOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayShippingOption/{shippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(220)]
    public async Task DeleteShippingOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayShippingOption/{shippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(230)]
    public async Task DeleteShippingOption_ShouldFailAndReturnNotFound_IfShippingOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusShippingOptionId = "bogusShippingOptionId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayShippingOption/{bogusShippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(240)]
    public async Task DeleteShippingOption_ShouldSucceedAndDeleteShippingOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string shippingOptionId = _chosenShippingOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayShippingOption/{shippingOptionId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayShippingOption/{shippingOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetShippingOptions_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayShippingOption/amount/10/includeDeactivated/true");

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
