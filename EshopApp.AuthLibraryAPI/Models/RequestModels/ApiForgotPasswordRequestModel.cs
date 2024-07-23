using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiForgotPasswordRequestModel
{
    [EmailAddress]
    [Required]
    public string? Email { get; set; }
}
