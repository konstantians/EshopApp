namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestOrder
{
    public string? Id { get; set; }
    public string? Comment { get; set; }
    public string? OrderStatus { get; set; } //Pending - Confirmed - Processing - Shipped - Delivered - Canceled - Refunded - Failed
    public decimal? FinalPrice { get; set; }
    public decimal? ShippingCostAtOrder { get; set; }
    public int? CouponDiscountPercentageAtOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? UserId { get; set; }
    public TestOrderAddress? OrderAddress { get; set; }
    public List<TestOrderItem> OrderItems { get; set; } = new List<TestOrderItem>();
    public TestPaymentDetails? PaymentDetails { get; set; }
    public string? ShippingOptionId { get; set; }
    public TestShippingOption? ShippingOption { get; set; }
    public string? UserCouponId { get; set; }
    public TestUserCoupon? UserCoupon { get; set; }
}
