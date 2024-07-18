namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiForgotPasswordRequestModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}
