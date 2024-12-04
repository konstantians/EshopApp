using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CartModels;

public class UpdateCartItemRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? CartItemId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "The quantity property have a value of 1 or greater.")]
    public int? Quantity { get; set; }
}
