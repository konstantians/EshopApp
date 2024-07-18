namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiSignInRequestModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? RememberMe { get; set; }
}
