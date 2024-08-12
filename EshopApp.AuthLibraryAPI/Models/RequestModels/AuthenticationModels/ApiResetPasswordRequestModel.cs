using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiResetPasswordRequestModel
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? Token { get; set; }
    [Required]
    public string? Password { get; set; }
}
