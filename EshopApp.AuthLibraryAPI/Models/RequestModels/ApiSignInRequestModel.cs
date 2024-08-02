using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiSignInRequestModel
{
    [EmailAddress]
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
