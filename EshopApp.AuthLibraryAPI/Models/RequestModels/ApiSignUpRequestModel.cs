using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiSignUpRequestModel
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
}
