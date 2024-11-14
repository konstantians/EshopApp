using EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantImageModels;

namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.VariantModels;
internal class TestCreateVariantRequestModel
{
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public int? UnitsInStock { get; set; }
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public string? ProductId { get; set; }
    public string? DiscountId { get; set; }
    public List<string> AttributeIds { get; set; } = new List<string>();
    public List<TestCreateVariantImageRequestModel> VariantImageRequestModels { get; set; } = new List<TestCreateVariantImageRequestModel>();

}
