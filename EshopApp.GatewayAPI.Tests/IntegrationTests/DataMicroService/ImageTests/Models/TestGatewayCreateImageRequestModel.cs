namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ImageTests.Models;
internal class TestGatewayCreateImageRequestModel
{
    public string? Name { get; set; }
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
