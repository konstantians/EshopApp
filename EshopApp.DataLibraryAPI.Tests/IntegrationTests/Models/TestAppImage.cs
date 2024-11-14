namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestAppImage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestVariantImage> VariantImages { get; set; } = new List<TestVariantImage>();

}
