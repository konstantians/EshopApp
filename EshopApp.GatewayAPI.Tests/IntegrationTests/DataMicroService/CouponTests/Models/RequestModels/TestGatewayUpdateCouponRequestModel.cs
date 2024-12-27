namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CouponTests.Models.RequestModels;
internal class TestGatewayUpdateCouponRequestModel
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? DiscountPercentage { get; set; }
    public int? UsageLimit { get; set; }
    public int? DefaultDateIntervalInDays { get; set; }
    public bool? IsDeactivated { get; set; }
    public string? TriggerEvent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public List<string>? UserCouponIds { get; set; }
}
