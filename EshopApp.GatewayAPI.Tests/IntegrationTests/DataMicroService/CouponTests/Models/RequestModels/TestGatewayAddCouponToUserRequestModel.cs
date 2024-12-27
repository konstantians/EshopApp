namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests.Models.RequestModels;
internal class TestGatewayAddCouponToUserRequestModel
{
    public string? Code { get; set; }
    public int? TimesUsed { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistInOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? UserId { get; set; }
    public string? CouponId { get; set; }
}
