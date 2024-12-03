namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.PaymentOptionModels;
internal class TestCreatePaymentOptionRequestModel
{
    public string? Name { get; set; }
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
}
