namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayUserCoupon
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public int? TimesUsed { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime? StartDate { get; set; } //nullable for implementation StartDate & ExpirationDate will always be filled
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? UserId { get; set; }
    public string? CouponId { get; set; }
    public GatewayCoupon? Coupon { get; set; }
    //public List<Order> Orders { get; set; } = new List<Order>(); will be added later
}
