using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAdmin.Models.RequestModels;

public class GatewayCreateUserRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [MaxLength(128, ErrorMessage = "Firstname can not exceed 128 characters")]
    public string? FirstName { get; set; }
    [MaxLength(128, ErrorMessage = "Firstname can not exceed 128 characters")]
    public string? LastName { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public bool SendEmailNotification { get; set; }
}
