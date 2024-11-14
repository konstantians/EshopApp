namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestVariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? VariantId { get; set; }
    public TestVariant? Variant { get; set; }
    public string? ImageId { get; set; }
    public TestAppImage? Image { get; set; }

}
