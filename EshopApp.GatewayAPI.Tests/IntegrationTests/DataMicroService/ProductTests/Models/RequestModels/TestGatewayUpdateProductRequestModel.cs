namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
internal class TestGatewayUpdateProductRequestModel
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? CategoryIds { get; set; }
    public List<string>? VariantIds { get; set; }
}
