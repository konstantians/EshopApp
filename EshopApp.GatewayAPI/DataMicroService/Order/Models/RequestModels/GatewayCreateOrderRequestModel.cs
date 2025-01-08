using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Order.Models.RequestModels;

public class GatewayCreateOrderRequestModel
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
    public List<GatewayOrderItemRequestModel> OrderItemRequestModels { get; set; } = new List<GatewayOrderItemRequestModel>();
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    [Required]
    [MaxLength(50)]
    public string? PaymentOptionId { get; set; }
    [Required]
    public string? ShippingOptionId { get; set; }
    public string? UserCouponId { get; set; }
    public bool IsFinal { get; set; } //this property here is only used by the gateway api and there is no need to be filled
}
