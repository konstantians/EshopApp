namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.DiscountTests.Models.RequestModels;
internal class TestGatewayCreateDiscountRequestModel
{
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
}
