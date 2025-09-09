using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.DiscountModels;

public class CreateDiscountViewModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? VariantIds { get; set; }
    [Required]
    [Range(1, 99, ErrorMessage = "Percentage must be between 1 and 99")]
    public int Percentage { get; set; }
    public bool IsDeactivated { get; set; }
    public bool IsActivated { get; set; }
}
