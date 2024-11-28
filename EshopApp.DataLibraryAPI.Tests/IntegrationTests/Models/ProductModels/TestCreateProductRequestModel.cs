using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantModels;

namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.ProductModels;
internal class TestCreateProductRequestModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string> CategoryIds { get; set; } = new List<string>();
    public TestCreateVariantRequestModel? CreateVariantRequestModel { get; set; }

}
