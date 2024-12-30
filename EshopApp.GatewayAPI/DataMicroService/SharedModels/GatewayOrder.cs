namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayOrder
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
    public GatewayOrderAddress? OrderAddress { get; set; }
    public List<GatewayOrderItem> OrderItems { get; set; } = new List<GatewayOrderItem>();
    public GatewayPaymentDetails? PaymentDetails { get; set; }
    public string? ShippingOptionId { get; set; }
    public GatewayShippingOption? ShippingOption { get; set; }
    public string? UserCouponId { get; set; }
    public GatewayUserCoupon? UserCoupon { get; set; }
}
