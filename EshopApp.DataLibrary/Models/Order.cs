namespace EshopApp.DataLibrary.Models;
public class Order
{
    public string? Id { get; set; }
    public string? Comment { get; set; }
    public string? OrderStatus { get; set; } //Pending - Confirmed - Processing - Shipped - Delivered - Canceled - Refunded - Failed
    public decimal? FinalPrice { get; set; }
    public decimal? ShippingCostAtOrder { get; set; }
    public int? CouponDiscountPercentageAtOrder { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? UserId { get; set; }
    public OrderAddress? OrderAddress { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public PaymentDetails? PaymentDetails { get; set; }
    public string? ShippingOptionId { get; set; }
    public ShippingOption? ShippingOption { get; set; }
    public string? UserCouponId { get; set; } //TODO change this to usercoupon
    public UserCoupon? UserCoupon { get; set; }
}

