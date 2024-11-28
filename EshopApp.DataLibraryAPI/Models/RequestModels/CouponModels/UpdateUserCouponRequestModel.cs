using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CouponModels;

public class UpdateUserCouponRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Code { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The times used property must be a non-negative integer.")]
    public int? TimesUsed { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistInOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
