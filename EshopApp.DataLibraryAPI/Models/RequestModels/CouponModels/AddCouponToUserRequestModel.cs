using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CouponModels;

public class AddCouponToUserRequestModel
{
    [MaxLength(50)]
    public string? Code { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The times used property must be a non-negative integer.")]
    public int? TimesUsed { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    [Required]
    [MaxLength(50)]
    public string? UserId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? CouponId { get; set; }
}
