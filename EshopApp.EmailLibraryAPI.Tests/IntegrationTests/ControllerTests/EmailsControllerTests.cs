using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.ResponseModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.ControllerTests;

[TestFixture]
[Category("Integration")]
[Author("konstantinos kinnas", "kinnaskonstantinos0@gmail.com")]
internal class EmailsControllerTests
{
    private HttpClient httpClient;
    private string? _chosenApiKey;
    private string? chosenEmailEntryId;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", "a7f3f1c6-3d2b-4e3a-8d70-4b6e8d6d53d8");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        _chosenApiKey = "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e";

        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();
    }

    [Test, Order(10)]
    public async Task SendEmailAndSaveEmailEntry_ShouldFailAndReturnUnauthorized_IfXAPIKEYHeaderIsMissing()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "Invalid Email Format";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("X-API-KEY");
    }

    [Test, Order(20)]
    public async Task SendEmailAndSaveEmailEntry_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "Invalid Email Format";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(30)]
    public async Task SendEmailAndSaveEmailEntry_ShouldReturnBadRequest_IfEmailFormatIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "Invalid Email Format";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(40)]
    public async Task SendEmailAndSaveEmailEntry_ShouldSendTheEmailAndCreateEmailEntry()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);
        string? emailContent = TestUtilitiesLibrary.EmailUtilities.ReadLastEmailFile(deleteEmailFile: true);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        emailContent.Should().NotBeNull();
    }

    [Test, Order(50)]
    public async Task GetEmailEntries_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(60)]
    public async Task GetEmailEntries_ShouldReturnEmailEntries()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestEmailResponseModel>? emailEntries = JsonSerializer.Deserialize<List<TestEmailResponseModel>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        emailEntries.Should().NotBeNull();
        emailEntries.Should().HaveCount(1);
        chosenEmailEntryId = emailEntries!.First().Id;
    }

    [Test, Order(70)]
    public async Task GetEmailEntry_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string emailEntryId = chosenEmailEntryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails/{emailEntryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(80)]
    public async Task GetEmailEntry_ShouldReturnNotFound_IfEmailEntryDoesNotExist()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        string bogusEmailEntryId = "bogusEmailId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails/{bogusEmailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(90)]
    public async Task GetEmailEntry_ShouldReturnOkAndEmailEntry()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        string emailEntryId = chosenEmailEntryId!;

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails/{emailEntryId}");
        string? responseBody = await response.Content.ReadAsStringAsync();
        TestEmailResponseModel? emailEntry = JsonSerializer.Deserialize<TestEmailResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        emailEntry.Should().NotBeNull();
        emailEntry!.Id.Should().Be(emailEntryId);
    }

    [Test, Order(100)]
    public async Task DeleteEmailEntry_ShouldFailAndReturnUnauthorized_IfAPIKeyIsInvalid()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "bogusKey");
        string emailEntryId = chosenEmailEntryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/emails/{emailEntryId}");
        string? errorMessage = await TestUtilitiesLibrary.JsonUtilities.GetSingleStringValueFromBody(response, "errorMessage");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        errorMessage.Should().NotBeNull();
        errorMessage.Should().Contain("Invalid");
    }

    [Test, Order(110)]
    public async Task DeleteEmailEntry_ShouldReturnNotFound_IfEmailEntryDoesNotExist()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        string bogusEmailEntryId = "bogusEmailId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/emails/{bogusEmailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(120)]
    public async Task DeleteEmailEntry_ShouldReturnNoContentAndDeleteEmailEntry()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "user_e1f7b8c0-3c79-4a1b-9e7a-9d8b1d4a5c6e");
        string emailEntryId = chosenEmailEntryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/emails/{emailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    //Rate Limit Test
    [Test, Order(300)]
    public async Task GetEmailEntries_ShouldFail_IfRateLimitIsExceededAndBypassHeaderNotFilledCorrectly()
    {
        //Arrange
        httpClient.DefaultRequestHeaders.Remove("X-Bypass-Rate-Limiting");
        httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _chosenApiKey);

        //Act
        HttpResponseMessage response = new();
        for (int i = 0; i < 101; i++)
            response = await httpClient.GetAsync($"api/emails");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        httpClient.Dispose();
        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();
    }
}
