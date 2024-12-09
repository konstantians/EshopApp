using System.ComponentModel.DataAnnotations;

namespace EshopApp.TransactionLibraryAPI.Models.RequestModels;

public class CreateCheckOutSessionRequestModel
{
    [MaxLength(50)]
    public string? PaymentMethodType { get; set; }
    [Required]
    public string? SuccessUrl { get; set; }
    [Required]
    public string? CancelUrl { get; set; }
    [Required]
    [EmailAddress]
    public string? CustomerEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }

    [Range(1, 99, ErrorMessage = "The discount percentage must be an integer value between 1 and 99.")]
    public int? CouponPercentage { get; set; } //optional

    [Required]
    [MaxLength(50)]
    public string? PaymentOptionName { get; set; }
    public string? PaymentOptionDescription { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The price of the payment option can not be negative.")]
    public decimal? PaymentOptionCostInEuro { get; set; }

    [Required]
    [MaxLength(50)]
    public string? ShippingOptionName { get; set; }
    public string? ShippingOptionDescription { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The price of the shipping option can not be negative.")]
    public decimal? ShippingOptionCostInEuro { get; set; }

    [Required]
    public List<CreateTransactionOrderItemRequestModel> CreateTransactionOrderItemRequestModels { get; set; } = new List<CreateTransactionOrderItemRequestModel>();
}
