namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models;
internal class TestGatewayUpdateVariantRequestModel
{
    public string? Id { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public int? UnitsInStock { get; set; }
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public string? DiscountId { get; set; }
    public List<string>? AttributeIds { get; set; }
    public List<string>? ImagesIds { get; set; }
    public string? ImageIdThatShouldBeThumbnail { get; set; }
}
