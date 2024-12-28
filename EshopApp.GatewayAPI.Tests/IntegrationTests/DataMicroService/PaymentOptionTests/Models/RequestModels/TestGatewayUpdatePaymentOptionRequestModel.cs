namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.PaymentOptionTests.Models.RequestModels;
internal class TestGatewayUpdatePaymentOptionRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public List<string>? PaymentDetailsIds { get; set; }
}
