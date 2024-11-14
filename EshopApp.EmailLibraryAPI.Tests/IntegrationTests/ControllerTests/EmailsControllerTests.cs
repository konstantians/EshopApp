using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.HelperMethods;
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
    private string? chosenEmailEntryId;

    [OneTimeSetUp]
    public async Task OnTimeSetup()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();

        await ResetDatabaseHelperMethods.ResetNoSqlEmailDatabaseAsync();
        EmailHelperMethods.DeleteAllEmailFiles();
    }

    [Test, Order(1)]
    public async Task SendEmailAndSaveEmailEntry_ShouldReturnBadRequest_IfEmailFormatIsInvalid()
    {
        //Arrange
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "Invalid Email Format";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test, Order(2)]
    public async Task SendEmailAndSaveEmailEntry_ShouldSendTheEmailAndCreateEmailEntry()
    {
        //Arrange
        TestEmailRequestModel testEmailRequestModel = new TestEmailRequestModel();
        testEmailRequestModel.Title = "Email Title";
        testEmailRequestModel.Message = "Email Message";
        testEmailRequestModel.Receiver = "kinnaskonstantinos0@gmail.com";

        //Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/emails", testEmailRequestModel);
        List<string>? emailLines = EmailHelperMethods.ReadLastEmailFile();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        emailLines.Should().NotBeNull();
        emailLines.Should().Contain(emailLine => emailLine.Contains(testEmailRequestModel.Title));
        emailLines.Should().Contain(emailLine => emailLine.Contains(testEmailRequestModel.Message));
        emailLines.Should().Contain(emailLine => emailLine.Contains(testEmailRequestModel.Receiver));
    }

    [Test, Order(3)]
    public async Task GetEmailEntries_ShouldReturnEmailEntries()
    {
        //Arrange

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails/");
        string? responseBody = await response.Content.ReadAsStringAsync();
        List<TestEmailResponseModel>? emailEntries = JsonSerializer.Deserialize<List<TestEmailResponseModel>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        emailEntries.Should().NotBeNull();
        emailEntries.Should().HaveCount(1);
        chosenEmailEntryId = emailEntries!.First().Id;
    }

    [Test, Order(4)]
    public async Task GetEmailEntry_ShouldReturnNotFound_IfEmailEntryDoesNotExist()
    {
        //Arrange
        string bogusEmailEntryId = "bogusEmailId";

        //Act
        HttpResponseMessage response = await httpClient.GetAsync($"api/emails/{bogusEmailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(5)]
    public async Task GetEmailEntry_ShouldReturnOkAndEmailEntry()
    {
        //Arrange
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

    [Test, Order(6)]
    public async Task DeleteEmailEntry_ShouldReturnNotFound_IfEmailEntryDoesNotExist()
    {
        //Arrange
        string bogusEmailEntryId = "bogusEmailId";

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/emails/{bogusEmailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test, Order(7)]
    public async Task DeleteEmailEntry_ShouldReturnNoContentAndDeleteEmailEntry()
    {
        //Arrange
        string emailEntryId = chosenEmailEntryId!;

        //Act
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/emails/{emailEntryId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        httpClient.Dispose();
        await ResetDatabaseHelperMethods.ResetNoSqlEmailDatabaseAsync();
        EmailHelperMethods.DeleteAllEmailFiles();
    }
}
