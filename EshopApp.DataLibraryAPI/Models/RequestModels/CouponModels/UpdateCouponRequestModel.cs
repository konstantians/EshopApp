using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CouponModels;

public class UpdateCouponRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Code { get; set; }
    public string? Description { get; set; }
    [Required]
    [Range(0, 99, ErrorMessage = "The discount percentage must be an integer value between 0 and 99.")]
    public int? DiscountPercentage { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The usage limit must be a non-negative integer.")]
    public int? UsageLimit { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The default date interval in days must be a non-negative integer.")]
    public int? DefaultDateIntervalInDays { get; set; }
    public bool? IsDeactivated { get; set; }
    [RegularExpression("OnSignUp|OnFirstOrder|OnEveryFiveOrders|OnEveryTenOrders|NoTrigger",
    ErrorMessage = "The trigger event must have one of the following values: OnSignUp, OnFirstOrder, OnEveryFiveOrders, OnEveryTenOrders, NoTrigger.")]
    public string? TriggerEvent { get; set; } //OnSignUp - OnFirstOrder - OnEveryFiveOrders - OnEveryTenOrders - NoTrigger
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public List<string>? UserCouponIds { get; set; }
}
