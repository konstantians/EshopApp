namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests.Models.RequestModels;
internal class TestGatewayUpdateImageRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
