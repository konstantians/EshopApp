namespace EshopApp.TransactionLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
internal class TestCreateTransactionOrderItemRequestModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? FinalUnitAmountInEuro { get; set; } //We will need to multiply by 100, because Stripe works with cents
    public int? Quantity { get; set; }
}
