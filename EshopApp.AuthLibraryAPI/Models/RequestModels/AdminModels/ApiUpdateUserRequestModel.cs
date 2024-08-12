using EshopApp.AuthLibrary.Models;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AdminModels;

public class ApiUpdateUserRequestModel
{
    public AppUser? AppUser { get; set; }
    public string? Password { get; set; }
    public bool ActivateEmail { get; set; } = true;
}
