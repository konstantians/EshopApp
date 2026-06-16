using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class OrderItemViewModel
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The quantity value must be a non-negative integer.")]
    public int? Quantity { get; set; }
    [Required]
    [MaxLength(50)]
    public string? VariantId { get; set; }
}
