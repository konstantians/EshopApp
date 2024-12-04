namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CartModels;
internal class TestCreateCartItemRequestModel
{
    public int Quantity { get; set; }
    public string? CartId { get; set; }
    public string? VariantId { get; set; }
}
