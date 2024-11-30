using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.OrderModels;

public class UpdateOrderRequestModel
{
    [MaxLength(50)]
    public string? Id { get; set; }
    public string? PaymentProcessorSessionId { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string? LastName { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Country { get; set; }
    [Required]
    [MaxLength(50)]
    public string? City { get; set; }
    [Required]
    [MaxLength(50)]
    public string? PostalCode { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Address { get; set; }
    [Required]
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    public bool? IsShippingAddressDifferent { get; set; }
    [MaxLength(50)]
    public string? AltFirstName { get; set; }
    [MaxLength(50)]
    public string? AltLastName { get; set; }
    [MaxLength(50)]
    public string? AltCountry { get; set; }
    [MaxLength(50)]
    public string? AltCity { get; set; }
    [MaxLength(50)]
    public string? AltPostalCode { get; set; }
    [MaxLength(50)]
    public string? AltAddress { get; set; }
    [MaxLength(50)]
    public string? AltPhoneNumber { get; set; }

    [MaxLength(50)]
    public string? PaymentStatus { get; set; }
    [MaxLength(5)]
    public string? PaymentCurrency { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The AmountPaidInCustomerCurrency property can not be negative")]
    public decimal? AmountPaidInCustomerCurrency { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The AmountPaidInEuro property can not be negative")]
    public decimal? AmountPaidInEuro { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The NetAmountPaidInEuro property can not be negative")]
    public decimal? NetAmountPaidInEuro { get; set; }
    public List<OrderItemRequestModel>? CreateOrderItemRequestModels { get; set; }
    public string? UserCouponId { get; set; }
}
