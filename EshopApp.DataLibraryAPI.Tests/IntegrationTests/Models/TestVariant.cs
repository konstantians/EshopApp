namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestVariant
{
    public string? Id { get; set; }
    public string? SKU { get; set; }
    public decimal? Price { get; set; }
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public int? UnitsInStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ProductId { get; set; }
    public TestProduct? Product { get; set; }
    public string? DiscountId { get; set; }
    public TestDiscount? Discount { get; set; }
    public List<TestAppAttribute> Attributes { get; set; } = new List<TestAppAttribute>();
    public List<TestVariantImage> VariantImages { get; set; } = new List<TestVariantImage>();

}
