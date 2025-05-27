using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AdminModels;

public class ApiCreateUserRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
    [MaxLength(128, ErrorMessage = "Firstname can not exceed 128 characters")]
    public string? FirstName { get; set; }
    [MaxLength(128, ErrorMessage = "Lastname can not exceed 128 characters")]
    public string? LastName { get; set; }

    [Required]
    public string? Password { get; set; }
}
