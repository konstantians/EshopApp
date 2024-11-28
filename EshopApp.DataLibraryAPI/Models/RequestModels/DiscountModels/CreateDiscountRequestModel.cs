using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.DiscountModels;

public class CreateDiscountRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    [Required]
    [Range(1, 99, ErrorMessage = "Percentage must be between 1 and 99")]
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
}
