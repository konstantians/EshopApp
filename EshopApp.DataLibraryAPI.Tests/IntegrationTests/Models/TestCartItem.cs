namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestCartItem
{
    public string? Id { get; set; }
    public int? Quantity { get; set; }
    public string? CartId { get; set; }
    public TestCart? Cart { get; set; }
    public string? VariantId { get; set; }
    public TestVariant? Variant { get; set; }
}
