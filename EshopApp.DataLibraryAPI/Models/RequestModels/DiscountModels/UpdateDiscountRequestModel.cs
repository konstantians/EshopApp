using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.DiscountModels;

public class UpdateDiscountRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    [Range(1, 99, ErrorMessage = "Percentage must be between 1 and 99")]
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? VariantIds { get; set; }
}
