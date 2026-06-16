using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class CreateOrderViewModel
{
    public string? Comment { get; set; }
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
    public string? UserId { get; set; } //this is not required, because it should support guest users also
    [Required]
    public List<OrderItemViewModel> OrderItemRequestModels { get; set; } = new List<OrderItemViewModel>();
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? PaymentOptionId { get; set; }
    [Required]
    public string? ShippingOptionId { get; set; }
    public string? UserCouponId { get; set; }
}
