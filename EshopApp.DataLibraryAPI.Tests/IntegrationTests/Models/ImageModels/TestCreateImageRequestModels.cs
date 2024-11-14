namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ImageModels;
internal class TestCreateImageRequestModels
{
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
