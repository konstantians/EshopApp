using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.PaymentOptionTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.PaymentOptionTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayPaymentOptionControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenPaymentOptionId;
    private string? _otherPaymentOptionId;
    private string? _otherPaymentOptionName;
    private string? _otherPaymentOptionNameAlias;

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

        /*************** Other Payment Option ***************/
        TestGatewayCreatePaymentOptionRequestModel testCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testCreatePaymentOptionRequestModel.Name = "OtherPaymentOption";
        testCreatePaymentOptionRequestModel.NameAlias = "card";
        testCreatePaymentOptionRequestModel.ExtraCost = 4;

        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testCreatePaymentOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayPaymentOption otherTestPaymentOption = JsonSerializer.Deserialize<TestGatewayPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        _otherPaymentOptionId = otherTestPaymentOption.Id;
        _otherPaymentOptionName = otherTestPaymentOption.Name;
        _otherPaymentOptionNameAlias = otherTestPaymentOption.NameAlias;
    }

    [Test, Order(10)]
    public async Task CreatePaymentOption_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreatePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel); //name property is missing and thus the request model is invalid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreatePaymentOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreatePaymentOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = _otherPaymentOptionName;
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(70)]
    public async Task CreatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionNameAlias()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = _otherPaymentOptionNameAlias;
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityNameAlias");
    }

    [Test, Order(80)]
    public async Task CreatePaymentOption_ShouldSucceedAndCreatePaymentOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreatePaymentOptionRequestModel testGatewayCreatePaymentOptionRequestModel = new TestGatewayCreatePaymentOptionRequestModel();
        testGatewayCreatePaymentOptionRequestModel.Name = "MyPaymentOption";
        testGatewayCreatePaymentOptionRequestModel.NameAlias = "cash";
        testGatewayCreatePaymentOptionRequestModel.Description = "My payment option description";
        testGatewayCreatePaymentOptionRequestModel.ExtraCost = 0;
        testGatewayCreatePaymentOptionRequestModel.IsDeactivated = false;
        testGatewayCreatePaymentOptionRequestModel.ExistsInOrder = false;

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayPaymentOption", testGatewayCreatePaymentOptionRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestGatewayPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testPaymentOption.Should().NotBeNull();
        testPaymentOption!.Id.Should().NotBeNull();
        testPaymentOption!.Name.Should().NotBeNull().And.Be(testGatewayCreatePaymentOptionRequestModel.Name);
        testPaymentOption!.NameAlias.Should().NotBeNull().And.Be(testGatewayCreatePaymentOptionRequestModel.NameAlias);
        testPaymentOption!.Description.Should().NotBeNull().And.Be(testGatewayCreatePaymentOptionRequestModel.Description);
        testPaymentOption!.ExtraCost.Should().NotBeNull().And.Be(testGatewayCreatePaymentOptionRequestModel.ExtraCost);
        testPaymentOption.IsDeactivated.Should().BeFalse();
        testPaymentOption.ExistsInOrder.Should().BeFalse();
        _chosenPaymentOptionId = testPaymentOption.Id;
    }

    [Test, Order(90)]
    public async Task GetPaymentOptions_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayPaymentOption/amount/10/includeDeactivated/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(100)]
    public async Task GetPaymentOptions_ShouldSucceedAndReturnPaymentOptions()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayPaymentOption/amount/10/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayPaymentOption>? testPaymentOptions = JsonSerializer.Deserialize<List<TestGatewayPaymentOption>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testPaymentOptions.Should().NotBeNull().And.HaveCount(2);
    }

    [Test, Order(110)]
    public async Task GetPaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayPaymentOption/{paymentOptionId}/includeDeactivated/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task GetPaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusPaymentOptionId = "bogusPaymentOptionId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayPaymentOption/{bogusPaymentOptionId}/includeDeactivated/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(130)]
    public async Task GetPaymentOption_ShouldSucceedAndReturnPaymentOption()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayPaymentOption/{paymentOptionId}/includeDeactivated/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestGatewayPaymentOption>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

    [Test, Order(140)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(150)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(160)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(170)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(180)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = "bogusPaymentOptionId";
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(190)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = _otherPaymentOptionName;
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(200)]
    public async Task UpdatePaymentOption_ShouldFailAndReturnBadRequest_IfDuplicatePaymentOptionNameAlias()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = _otherPaymentOptionNameAlias;
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityNameAlias");
    }

    [Test, Order(210)]
    public async Task UpdatePaymentOption_ShouldSucceedAndUpdatePaymentOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdatePaymentOptionRequestModel testGatewayUpdatePaymentOptionRequestModel = new TestGatewayUpdatePaymentOptionRequestModel();
        testGatewayUpdatePaymentOptionRequestModel.Id = _chosenPaymentOptionId;
        testGatewayUpdatePaymentOptionRequestModel.NameAlias = "googlePay";
        testGatewayUpdatePaymentOptionRequestModel.Name = "MyPaymentOptionUpdated";
        testGatewayUpdatePaymentOptionRequestModel.Description = "My payment option description updated";
        testGatewayUpdatePaymentOptionRequestModel.ExtraCost = 10;

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayPaymentOption", testGatewayUpdatePaymentOptionRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayPaymentOption/{_chosenPaymentOptionId}/includeDeactivated/true");
        string? getResponseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayPaymentOption? testPaymentOption = JsonSerializer.Deserialize<TestGatewayPaymentOption>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testPaymentOption!.Name.Should().NotBeNull().And.Be(testGatewayUpdatePaymentOptionRequestModel.Name);
        testPaymentOption!.NameAlias.Should().NotBeNull().And.Be(testGatewayUpdatePaymentOptionRequestModel.NameAlias);
        testPaymentOption!.Description.Should().NotBeNull().And.Be(testGatewayUpdatePaymentOptionRequestModel.Description);
        testPaymentOption!.ExtraCost.Should().NotBeNull().And.Be(testGatewayUpdatePaymentOptionRequestModel.ExtraCost);
        testPaymentOption.IsDeactivated.Should().BeFalse();
        testPaymentOption.ExistsInOrder.Should().BeFalse();
        //testPaymentOption.PaymentDetails.Should().NotBeNull().And.HaveCount(0); TODO add this later
    }

    [Test, Order(220)]
    public async Task DeletePaymentOption_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{paymentOptionId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(230)]
    public async Task DeletePaymentOption_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{paymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(240)]
    public async Task DeletePaymentOption_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{paymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(250)]
    public async Task DeletePaymentOption_ShouldFailAndReturnNotFound_IfPaymentOptionNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string bogusPaymentOptionId = "bogusPaymentOptionId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{bogusPaymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(260)]
    public async Task DeletePaymentOption_ShouldSucceedAndDeletePaymentOption()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string paymentOptionId = _chosenPaymentOptionId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{paymentOptionId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayPaymentOption/{paymentOptionId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(500)]
    public async Task GetPaymentOptions_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayPaymentOption/amount/10/includeDeactivated/true");

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
