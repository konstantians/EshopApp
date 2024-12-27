namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.DiscountTests.Models.RequestModels;
internal class TestGatewayUpdateDiscountRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? VariantIds { get; set; }
}
