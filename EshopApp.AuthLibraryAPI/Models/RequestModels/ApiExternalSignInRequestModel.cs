using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiExternalSignInRequestModel
{
    [Required]
    public string? IdentityProviderName { get; set; }
    [Required]
    public string? ReturnUrl { get; set; }
}
