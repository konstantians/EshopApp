namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.AttributeModels;
internal class TestUpdateAttributeRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? VariantIds { get; set; }
}
