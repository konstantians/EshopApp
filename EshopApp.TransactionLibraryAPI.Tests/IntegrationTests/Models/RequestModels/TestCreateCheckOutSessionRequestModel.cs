namespace EshopApp.TransactionLibraryAPI.Tests.IntegrationTests.Models.RequestModels;
internal class TestCreateCheckOutSessionRequestModel
{
    public string? PaymentMethodType { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? CouponPercentage { get; set; } //optional
    public string? PaymentOptionName { get; set; }
    public string? PaymentOptionDescription { get; set; }
    public decimal? PaymentOptionCostInEuro { get; set; }
    public string? ShippingOptionName { get; set; }
    public string? ShippingOptionDescription { get; set; }
    public decimal? ShippingOptionCostInEuro { get; set; }
    public List<TestCreateTransactionOrderItemRequestModel> CreateTransactionOrderItemRequestModels { get; set; } = new List<TestCreateTransactionOrderItemRequestModel>();

}
