namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiResetPasswordRequestModel
{
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? Password { get; set; }
}
