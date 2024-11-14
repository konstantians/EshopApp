namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ImageModels;
internal class TestUpdateImageRequestModels
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
