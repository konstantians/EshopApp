using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CartViewModels;

public class AddItemToCartViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The quantity property have a value of 1 or greater.")]
    public int Quantity { get; set; }
    [MaxLength(50)]
    public string? CartId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? VariantId { get; set; }
    public bool ShouldUpdate { get; set; }
}
