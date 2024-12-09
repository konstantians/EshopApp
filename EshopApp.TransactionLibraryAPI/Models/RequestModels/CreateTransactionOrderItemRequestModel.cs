using System.ComponentModel.DataAnnotations;

namespace EshopApp.TransactionLibraryAPI.Models.RequestModels;

public class CreateTransactionOrderItemRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [MaxLength(75)]
    public string? ImageUrl { get; set; }
    [Required]
    [Range(0.000001, int.MaxValue, ErrorMessage = "The price of the order item can not be negative.")]
    public decimal? FinalUnitAmountInEuro { get; set; } //We will need to multiply by 100, because Stripe works with cents
    [Required]
    [Range(0.000001, int.MaxValue, ErrorMessage = "The quantity value must be a non-negative integer.")]
    public int? Quantity { get; set; }
}
