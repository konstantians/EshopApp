namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CouponModels;
internal class TestAddCouponToUserRequestModel
{
    public string? Code { get; set; }
    public int? TimesUsed { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? UserId { get; set; }
    public string? CouponId { get; set; }

}
