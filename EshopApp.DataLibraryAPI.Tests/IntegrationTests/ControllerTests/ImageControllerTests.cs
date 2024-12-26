using EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ImageModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class ImageControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? _chosenImageId;
    private string? _otherImageName;

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

        /*************** Other Image ***************/
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "OtherImage";
        testCreateImageRequestModel.ImagePath = "OtherPath";
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        string responseBody = await response.Content.ReadAsStringAsync();
        _otherImageName = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Name;
    }

    [Test, Order(10)]
    public async Task CreateImage_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "MyImage";
        testCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task CreateImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "MyImage";
        testCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task CreateImage_ShouldFailAndReturnBadRequest_IfInvalidRequestModelFormatInImageEntity()
    {
        //Arrange

        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel); //The name value of the image is missing and thus the request model is not valid

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task CreateImage_ShouldSucceedAndCreateImage()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "MyImage";
        testCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppImage? testImage = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        testImage.Should().NotBeNull();
        testImage!.Id.Should().NotBeNull();
        testImage!.Name.Should().NotBeNull().And.Be(testCreateImageRequestModel.Name);
        testImage!.ImagePath.Should().NotBeNull().And.StartWith(testCreateImageRequestModel.Name + "_");
        _chosenImageId = testImage.Id;
    }

    [Test, Order(50)]
    public async Task CreateImage_ShouldFailAndReturnBadRequest_IfDuplicateImageName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestCreateImageRequestModels testCreateImageRequestModel = new TestCreateImageRequestModels();
        testCreateImageRequestModel.Name = "MyImage";
        testCreateImageRequestModel.ImagePath = "Path";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/image", testCreateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(60)]
    public async Task GetImages_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/image/amount/10/includeSoftDeleted/true"); //here 10 is just an arbitraty value for the amount
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(70)]
    public async Task GetImages_ShouldSucceedAndReturnImages()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = await httpClient.GetAsync("api/image/amount/10/includeSoftDeleted/true"); //here 10 is just an arbitraty value for the amount
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestAppImage>? testImages = JsonSerializer.Deserialize<List<TestAppImage>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testImages.Should().NotBeNull().And.HaveCount(2); //one we created on the setup and one in the previous tests
    }

    [Test, Order(80)]
    public async Task GetImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/image/id/{imageId}/includeSoftDeleted/true");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(90)]
    public async Task GetImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusImageId = "bogusImageId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/image/id/{bogusImageId}/includeSoftDeleted/true");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(100)]
    public async Task GetImage_ShouldSucceedAndReturnImage()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/image/id/{imageId}/includeSoftDeleted/true");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestAppImage? testImage = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        testImage.Should().NotBeNull();
        testImage!.Id.Should().NotBeNull().And.Be(imageId);
        testImage!.Name.Should().NotBeNull();
        testImage!.ImagePath.Should().NotBeNull();
    }

    [Test, Order(110)]
    public async Task UpdateImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestUpdateImageRequestModels testUpdateImageRequestModel = new TestUpdateImageRequestModels();
        testUpdateImageRequestModel.Id = _chosenImageId;
        testUpdateImageRequestModel.Name = "MyImageUpdated";
        testUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/image", testUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(120)]
    public async Task UpdateImage_ShouldFailAndReturnNotFound_IfInvalidRequestModelFormat()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateImageRequestModels testUpdateImageRequestModel = new TestUpdateImageRequestModels();
        testUpdateImageRequestModel.Name = "MyImageUpdated";
        testUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/image", testUpdateImageRequestModel); //here the id is missing, which is required for the update

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(130)]
    public async Task UpdateImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateImageRequestModels testUpdateImageRequestModel = new TestUpdateImageRequestModels();
        testUpdateImageRequestModel.Id = "bogusImageId";
        testUpdateImageRequestModel.Name = "MyImageUpdated";
        testUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/image", testUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        errorMessage.Should().NotBeNull().And.Be("EntityNotFoundWithGivenId");
    }

    [Test, Order(140)]
    public async Task UpdateImage_ShouldFailAndReturnBadRequest_IfDuplicateImageName()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateImageRequestModels testUpdateImageRequestModel = new TestUpdateImageRequestModels();
        testUpdateImageRequestModel.Id = _chosenImageId;
        testUpdateImageRequestModel.Name = _otherImageName;
        testUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/image", testUpdateImageRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorMessage.Should().NotBeNull().And.Be("DuplicateEntityName");
    }

    [Test, Order(150)]
    public async Task UpdateImage_ShouldSucceedAndUpdateImage()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        TestUpdateImageRequestModels testUpdateImageRequestModel = new TestUpdateImageRequestModels();
        testUpdateImageRequestModel.Id = _chosenImageId;
        testUpdateImageRequestModel.Name = "MyImageUpdated";
        testUpdateImageRequestModel.ImagePath = "PathUpdated";

        //Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync("api/image", testUpdateImageRequestModel);
        HttpResponseMessage getResponse = await httpClient.GetAsync($"api/image/id/{_chosenImageId}/includesoftdeleted/true");
        string? responseBody = await getResponse.Content.ReadAsStringAsync();
        TestAppImage? testImage = JsonSerializer.Deserialize<TestAppImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        testImage!.Name.Should().NotBeNull().And.Be("MyImageUpdated");
        testImage!.ImagePath.Should().NotBeNull().And.Be("PathUpdated");
    }

    //TODO when there is order do the soft deleted thing here, since now there is no reason to do it...

    [Test, Order(210)]
    public async Task DeleteImage_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/image/{imageId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull().And.Contain("Invalid");
    }

    [Test, Order(220)]
    public async Task DeleteImage_ShouldFailAndReturnNotFound_IfImageNotInSystem()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string bogusImageId = "bogusImageId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/image/{bogusImageId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(230)]
    public async Task DeleteImage_ShouldSucceedAndDeleteImage()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);
        string imageId = _chosenImageId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/image/{imageId}");
        HttpResponseMessage secondResponse = await httpClient.DeleteAsync($"api/image/{imageId}");

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
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync("api/image/amount/10/includeSoftDeleted/true");

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
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );
    }
}
