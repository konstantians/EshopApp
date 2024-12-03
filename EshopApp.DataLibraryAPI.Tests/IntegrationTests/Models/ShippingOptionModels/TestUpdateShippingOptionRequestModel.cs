namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.ShippingOptionModels;
internal class TestUpdateShippingOptionRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public List<string>? OrderIds { get; set; }

}
