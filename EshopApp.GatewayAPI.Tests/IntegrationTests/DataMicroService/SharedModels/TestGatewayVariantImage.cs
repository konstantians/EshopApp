namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayVariantImage
{
    public string? Id { get; set; }
    public bool IsThumbNail { get; set; }
    public string? VariantId { get; set; }
    public TestGatewayVariant? Variant { get; set; }
    public string? ImageId { get; set; }
    public TestGatewayAppImage? Image { get; set; }
}
