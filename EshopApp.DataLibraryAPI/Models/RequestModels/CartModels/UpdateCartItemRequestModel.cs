using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CartModels;

public class UpdateCartItemRequestModel
{
    [Required]
    public string? CartId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "The quantity property have a value of 1 or greater.")]
    public int? Quantity { get; set; }
    [MaxLength(50)]
    public string? VariantId { get; set; }
}
