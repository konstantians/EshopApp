using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CartViewModels;

public class AddItemToCartViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "The quantity property have a value of 1 or greater.")]
    public int Quantity { get; set; }
    [Required]
    [MaxLength(50)]
    public string? VariantId { get; set; }

    //This is filled by the endpoint
    public string? CartId { get; set; }
}
