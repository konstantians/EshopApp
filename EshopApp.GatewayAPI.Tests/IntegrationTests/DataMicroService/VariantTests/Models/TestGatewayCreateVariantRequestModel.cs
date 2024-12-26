using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
internal class TestGatewayCreateVariantRequestModel
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
    public List<TestGatewayCreateVariantImageRequestModel> VariantImageRequestModels { get; set; } = new List<TestGatewayCreateVariantImageRequestModel>();
}
