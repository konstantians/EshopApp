using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiSignUpRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; } //in this app the username is the same as the email
    [Required]
    public string? Password { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
}
