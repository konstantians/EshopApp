using EshopApp.AuthLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AdminModels;

public class ApiUpdateUserRequestModel
{
    [Required]
    public AppUser? AppUser { get; set; }
    public string? Password { get; set; }
    public bool ActivateEmail { get; set; } = true;
}
