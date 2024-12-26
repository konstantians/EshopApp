using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Coupon.Models.RequestModels;

public class GatewayAddCouponToUserRequestModel
{
    [MaxLength(50)]
    public string? Code { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The times used property must be a non-negative integer.")]
    public int? TimesUsed { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistInOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    [Required]
    [MaxLength(50)]
    public string? UserId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? CouponId { get; set; }
}
