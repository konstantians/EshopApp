namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ProductModels;
internal class TestUpdateProductRequestModel
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? CategoryIds { get; set; }
    public List<string>? VariantIds { get; set; }
}
