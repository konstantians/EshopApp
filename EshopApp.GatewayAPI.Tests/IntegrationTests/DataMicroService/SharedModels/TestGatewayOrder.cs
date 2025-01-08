namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayOrder
{
    public string? Id { get; set; }
    public string? Comment { get; set; }
    public string? OrderStatus { get; set; } //Pending - Confirmed - Processing - Shipped - Delivered - Canceled - RefundPending - RefundFailed - Refunded - Failed
    public decimal? FinalPrice { get; set; }
    public decimal? ShippingCostAtOrder { get; set; }
    public int? CouponDiscountPercentageAtOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? UserId { get; set; }
    public TestGatewayOrderAddress? OrderAddress { get; set; }
    public List<TestGatewayOrderItem> OrderItems { get; set; } = new List<TestGatewayOrderItem>();
    public TestGatewayPaymentDetails? PaymentDetails { get; set; }
    public string? ShippingOptionId { get; set; }
    public TestGatewayShippingOption? ShippingOption { get; set; }
    public string? UserCouponId { get; set; }
    public TestGatewayUserCoupon? UserCoupon { get; set; }
}
