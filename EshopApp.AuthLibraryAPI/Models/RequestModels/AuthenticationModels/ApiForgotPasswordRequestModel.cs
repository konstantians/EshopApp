using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiForgotPasswordRequestModel
{
    [EmailAddress]
    [Required]
    public string? Email { get; set; }
}
