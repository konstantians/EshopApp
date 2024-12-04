namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestUserCoupon
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
    public TestCoupon? Coupon { get; set; }
    public List<TestOrder> Orders { get; set; } = new List<TestOrder>();
}
