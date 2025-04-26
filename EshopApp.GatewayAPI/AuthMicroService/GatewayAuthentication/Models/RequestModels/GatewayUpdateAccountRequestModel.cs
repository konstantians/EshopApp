using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;

public class GatewayUpdateAccountRequestModel
{
    [MaxLength(128, ErrorMessage = "Firstname can not exceed 128 characters")]
    public string? FirstName { get; set; }
    [MaxLength(128, ErrorMessage = "Lastname can not exceed 128 characters")]
    public string? LastName { get; set; }
    [RegularExpression(@"^\+?\d{1,4}[\s\-]?\(?\d{1,3}\)?[\s\-]?\d{1,4}[\s\-]?\d{1,4}[\s\-]?\d{1,4}$", ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }
}
