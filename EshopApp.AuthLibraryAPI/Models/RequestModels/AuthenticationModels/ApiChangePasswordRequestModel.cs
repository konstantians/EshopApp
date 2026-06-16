using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiChangePasswordRequestModel
{
    public string? CurrentPassword { get; set; }
    [Required]
    public string? NewPassword { get; set; }
}
