using EshopApp.AuthLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AdminModels;

public class ApiCreateUserRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public string? Password { get; set; }
}
