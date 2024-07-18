namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiChangePasswordRequestModel
{
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}
