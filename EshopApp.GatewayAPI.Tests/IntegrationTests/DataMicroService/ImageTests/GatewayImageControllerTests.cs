using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests.Models.RequestModels;
using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class GatewayImageControllerTests
{
    private HttpClient httpClient;
    private int waitTimeInMillisecond = 5000;
    private string? _chosenApiKey;
    private string? _userAccessToken;
    private string? _managerAccessToken;
    private string? _adminAccessToken;
    private string? _chosenImageId;
    private string? _otherImageName;

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

        /*************** Other Image ***************/
        TestGatewayCreateImageRequestModel testCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testCreateImageRequestModel.Name = "OtherImage";
        testCreateImageRequestModel.ImagePath = "OtherPath";
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testCreateImageRequestModel);
        string responseBody = await response.Content.ReadAsStringAsync();
        _otherImageName = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateImage_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = "MyImage";
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = "MyImage";
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateImage_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInImageEntity()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel); //The name value of the image is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateImage_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer BogusAccessToken");
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = "MyImage";
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(50)]
    public async Task CreateImage_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = "MyImage";
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(60)]
    public async Task CreateImage_ShouldFailAndReturnBadRequest_IfDuplicateImageName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = _otherImageName;
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(70)]
    public async Task CreateImage_ShouldSucceedAndCreateImage()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayCreateImageRequestModel testGatewayCreateImageRequestModel = new TestGatewayCreateImageRequestModel();
        testGatewayCreateImageRequestModel.Name = "MyImage";
        testGatewayCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/gatewayImage", testGatewayCreateImageRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppImage? testImage = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testImage.Should().NotBeNull();
        testImage!.Id.Should().NotBeNull();
        testImage!.Name.Should().NotBeNull().And.Be(testGatewayCreateImageRequestModel.Name);
        //testImage!.ImagePath.Should().NotBeNull().And.StartWith(testGatewayCreateImageRequestModel.Name + "_");
        _chosenImageId = testImage.Id;
    }

    [Test, Order(80)]
    public async Task GetImages_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayImage/amount/10/includeSoftDeleted/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetImages_ShouldSucceedAndReturnImages()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/gatewayImage/amount/10/includeSoftDeleted/true"); //here 10 is just an arbitraty value for the amount
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestGatewayAppImage>? testImages = JsonSerializer.Deserialize<List<TestGatewayAppImage>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testImages.Should().NotBeNull().And.HaveCount(2); //one we created on the setup and one in the previous tests
    }

    [Test, Order(100)]
    public async Task GetImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayImage/id/{imageId}/includeSoftDeleted/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task GetImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusImageId = "bogusImageId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayImage/id/{bogusImageId}/includeSoftDeleted/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task GetImage_ShouldSucceedAndReturnImage()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/gatewayImage/id/{imageId}/includeSoftDeleted/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestGatewayAppImage? testImage = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testImage.Should().NotBeNull();
        testImage!.Id.Should().NotBeNull().And.Be(imageId);
        testImage!.Name.Should().NotBeNull();
        testImage!.ImagePath.Should().NotBeNull();
    }

    [Test, Order(130)]
    public async Task UpdateImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = _chosenImageId;
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(140)]
    public async Task UpdateImage_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(150)]
    public async Task UpdateImage_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = _chosenImageId;
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(160)]
    public async Task UpdateImage_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = _chosenImageId;
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(170)]
    public async Task UpdateImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = "bogusImageId";
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(180)]
    public async Task UpdateImage_ShouldFailAndReturnBadRequest_IfDuplicateImageName()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = _chosenImageId;
        testGatewayUpdateImageRequestModel.Name = _otherImageName;
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(190)]
    public async Task UpdateImage_ShouldSucceedAndUpdateImage()
    {
        //Arrange
        TestGatewayUpdateImageRequestModel testGatewayUpdateImageRequestModel = new TestGatewayUpdateImageRequestModel();
        testGatewayUpdateImageRequestModel.Id = _chosenImageId;
        testGatewayUpdateImageRequestModel.Name = "MyImageUpdated";
        testGatewayUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/gatewayImage", testGatewayUpdateImageRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/gatewayImage/id/{_chosenImageId}/includesoftdeleted/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestGatewayAppImage? testImage = JsonSerializer.Deserialize<TestGatewayAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testImage!.Name.Should().NotBeNull().And.Be("MyImageUpdated");
        testImage!.ImagePath.Should().NotBeNull().And.Be("PathUpdated");
    }

    [Test, Order(200)]
    public async Task DeleteImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, "bogusKey", _adminAccessToken);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayImage/{imageId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(210)]
    public async Task DeleteImage_ShouldFailAndReturnUnauthorized_IfUserNotAuthenticated()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, "Bearer bogusAccessToken");
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayImage/{imageId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(220)]
    public async Task DeleteImage_ShouldFailAndReturnForbidden_IfUserAuthenticatedButDoesNotHaveCorrectClaims()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _userAccessToken);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayImage/{imageId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test, Order(230)]
    public async Task DeleteImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusImageId = "bogusImageId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayImage/{bogusImageId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(240)]
    public async Task DeleteImage_ShouldSucceedAndDeleteImage()
    {
        //Arrange
        TestUtilitiesLibrary.CommonTestProcedures.SetDefaultHttpHeaders(httpClient, _chosenApiKey, _adminAccessToken);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/gatewayImage/{imageId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/gatewayImage/{imageId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetImages_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/gatewayImage/amount/10/includeSoftDeleted/true");

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
