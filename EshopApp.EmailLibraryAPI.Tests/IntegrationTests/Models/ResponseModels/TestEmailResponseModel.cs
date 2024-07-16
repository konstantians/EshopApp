namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.ResponseModels;

internal class TestEmailResponseModel
{
    public string? Id { get; set; }
    public DateTime SentAt { get; set; }
    public string? Receiver { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}
