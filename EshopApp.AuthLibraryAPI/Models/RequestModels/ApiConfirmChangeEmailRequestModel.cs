using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiConfirmChangeEmailRequestModel
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    [EmailAddress]
    public string? NewEmail { get; set; }
    [Required]
    public string? ChangeEmailToken { get; set; }
}
