namespace EshopApp.DataLibrary.Models;
public class UserCoupon
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public int? TimesUsed { get; set; }
    public DateTime? StartDate { get; set; } //nullable for implementation StartDate & ExpirationDate will always be filled
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? UserId { get; set; }
    public string? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
}
