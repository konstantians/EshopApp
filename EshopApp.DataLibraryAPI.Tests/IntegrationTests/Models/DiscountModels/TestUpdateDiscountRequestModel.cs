namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.DiscountModels;
internal class TestUpdateDiscountRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? VariantIds { get; set; }
}
