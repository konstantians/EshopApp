namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayCoupon
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? DiscountPercentage { get; set; }
    public int? UsageLimit { get; set; }
    public int? DefaultDateIntervalInDays { get; set; }
    public bool? IsUserSpecific { get; set; }
    public bool? IsDeactivated { get; set; }
    public string? TriggerEvent { get; set; } //OnSignUp - OnFirstOrder - OnEveryFiveOrders - OnEveryTenOrders - NoTrigger
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<GatewayUserCoupon> UserCoupons { get; set; } = new List<GatewayUserCoupon>();
}
