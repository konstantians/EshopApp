namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CouponModels;
internal class TestUpdateUserCouponRequestModel
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public int? TimesUsed { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
