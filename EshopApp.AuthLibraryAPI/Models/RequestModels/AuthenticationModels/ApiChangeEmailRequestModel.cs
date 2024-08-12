using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiChangeEmailRequestModel
{
    [Required]
    public string? NewEmail { get; set; }
}
